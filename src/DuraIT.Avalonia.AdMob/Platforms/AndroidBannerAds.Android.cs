using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Android.App;
using Android.Gms.Ads;
using Xamarin.Google.UserMesssagingPlatform;

namespace DuraIT.Avalonia.AdMob;

/// <summary>
/// Android startup helpers for the AdMob banner integration: requests GDPR/UMP consent and, once
/// consent allows it, initializes the Google Mobile Ads SDK.
/// </summary>
internal static class AndroidBannerAds
{
    private static Task<bool>? _readyTask;
    private static volatile bool _privacyOptionsRequired;
    private static WeakReference<Activity>? _currentActivity;

    /// <summary>
    /// Gets or sets the activity currently hosting a banner, captured when a <see cref="BannerAd"/>
    /// is created. Held weakly so a destroyed activity — for example one replaced by a configuration
    /// change — is not retained for the life of the process. The privacy options form is presented
    /// from the DI service, which has no view of its own, so it presents on this activity.
    /// </summary>
    internal static Activity? CurrentActivity
    {
        get =>
            _currentActivity is not null && _currentActivity.TryGetTarget(out var activity)
                ? activity
                : null;
        set => _currentActivity = value is null ? null : new WeakReference<Activity>(value);
    }

    /// <summary>
    /// Gets a value indicating whether UMP reports that a privacy options entry point is required for
    /// the current user (for example an EEA user under GDPR). Only meaningful after
    /// <see cref="EnsureReadyAsync"/> has requested consent; defaults to <see langword="false"/>.
    /// </summary>
    internal static bool IsPrivacyOptionsRequired => _privacyOptionsRequired;

    /// <summary>
    /// Ensures consent has been requested — presenting a form if regulation requires one the user
    /// hasn't answered yet — and initializes the Google Mobile Ads SDK once
    /// <c>ConsentInformation.CanRequestAds()</c> allows it. Safe to call from multiple
    /// <see cref="BannerAd"/> instances: the underlying request and initialization run once per
    /// process. Must be called on the UI thread with the activity hosting the Avalonia view.
    /// </summary>
    /// <param name="activity">
    /// The activity to present the consent form on, if one is required.
    /// </param>
    /// <returns>
    /// <see langword="true"/> once ads may be requested; <see langword="false"/> if consent is
    /// required and was not obtained.
    /// </returns>
    internal static Task<bool> EnsureReadyAsync(Activity activity) =>
        _readyTask ??= RequestConsentAndInitializeAsync(activity);

    /// <summary>
    /// Re-opens the privacy options form on the activity that last hosted a banner. A no-op that
    /// completes immediately when no such activity is available.
    /// </summary>
    /// <returns>
    /// A task that completes when the form is dismissed.
    /// </returns>
    internal static Task ShowPrivacyOptionsAsync() =>
        CurrentActivity is { } activity ? ShowPrivacyOptionsAsync(activity) : Task.CompletedTask;

    /// <summary>
    /// Re-opens the privacy options form so the user can change a previously made consent choice.
    /// Google requires apps using UMP to offer this somewhere in their settings once real (not test)
    /// ads ship.
    /// </summary>
    /// <param name="activity">
    /// The activity to present the form on.
    /// </param>
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The listener's lifetime is owned by the Android callback machinery until the form is dismissed; disposing it here would break the callback."
    )]
    private static Task ShowPrivacyOptionsAsync(Activity activity)
    {
        var tcs = new TaskCompletionSource();
        UserMessagingPlatform.ShowPrivacyOptionsForm(
            activity,
            new ConsentFormDismissedListener(tcs)
        );
        return tcs.Task;
    }

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "ConsentRequestParameters and the listener instances are owned by the Android UMP callback machinery until their callbacks fire; disposing them here would break the callbacks."
    )]
    private static async Task<bool> RequestConsentAndInitializeAsync(Activity activity)
    {
        var consentInformation = UserMessagingPlatform.GetConsentInformation(activity);

        var parameters = new ConsentRequestParameters.Builder()
            .SetTagForUnderAgeOfConsent(AdMobRuntime.Options.TagForUnderAgeOfConsent)
            .Build();

        var updateTcs = new TaskCompletionSource();
        consentInformation.RequestConsentInfoUpdate(
            activity,
            parameters,
            new ConsentInfoUpdateSuccessListener(updateTcs),
            new ConsentInfoUpdateFailureListener(updateTcs)
        );
        await updateTcs.Task;

        // Cache whether a privacy options entry point applies, so the About screen can offer a
        // "manage consent" control only to users a form was shown to (e.g. EEA under GDPR).
        _privacyOptionsRequired =
            consentInformation.PrivacyOptionsRequirementStatus?.Equals(
                ConsentInformationPrivacyOptionsRequirementStatus.Required
            ) == true;

        // No-ops internally if no form is required (already obtained, or not required at all).
        var formTcs = new TaskCompletionSource();
        UserMessagingPlatform.LoadAndShowConsentFormIfRequired(
            activity,
            new ConsentFormDismissedListener(formTcs)
        );
        await formTcs.Task;

        if (!consentInformation.CanRequestAds())
        {
            return false;
        }

        MobileAds.Initialize(activity);
        return true;
    }

    private sealed class ConsentInfoUpdateSuccessListener
        : Java.Lang.Object,
            IConsentInformationOnConsentInfoUpdateSuccessListener
    {
        private readonly TaskCompletionSource _tcs;

        public ConsentInfoUpdateSuccessListener(TaskCompletionSource tcs) => _tcs = tcs;

        public void OnConsentInfoUpdateSuccess() => _tcs.TrySetResult();
    }

    private sealed class ConsentInfoUpdateFailureListener
        : Java.Lang.Object,
            IConsentInformationOnConsentInfoUpdateFailureListener
    {
        private readonly TaskCompletionSource _tcs;

        public ConsentInfoUpdateFailureListener(TaskCompletionSource tcs) => _tcs = tcs;

        // Fail open: a transient consent-service error shouldn't permanently block ad requests. Let
        // the caller proceed to check CanRequestAds(), which may already be true from a cached prior
        // grant (Google's own guidance: don't let a network hiccup block ad serving).
        public void OnConsentInfoUpdateFailure(FormError p0) => _tcs.TrySetResult();
    }

    private sealed class ConsentFormDismissedListener
        : Java.Lang.Object,
            IConsentFormOnConsentFormDismissedListener
    {
        private readonly TaskCompletionSource _tcs;

        public ConsentFormDismissedListener(TaskCompletionSource tcs) => _tcs = tcs;

        public void OnConsentFormDismissed(FormError? p0) => _tcs.TrySetResult();
    }
}
