using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using MT.GMA.iOS;
using MT.UMP.iOS;
using UIKit;

namespace DuraIT.Avalonia.AdMob.Platforms;

/// <summary>
/// iOS startup helpers for the AdMob banner integration: requests GDPR/UMP consent and, once consent
/// allows it, initializes the Google Mobile Ads SDK.
/// </summary>
internal static class IosBannerAds
{
    private static Task<bool>? _readyTask;
    private static volatile bool _privacyOptionsRequired;

    /// <summary>
    /// Gets a value indicating whether UMP reports that a privacy options entry point is required for
    /// the current user (for example an EEA user under GDPR). Only meaningful after
    /// <see cref="EnsureReadyAsync" /> has requested consent; defaults to <see langword="false" />.
    /// </summary>
    internal static bool IsPrivacyOptionsRequired => _privacyOptionsRequired;

    /// <summary>
    /// Ensures consent has been requested — presenting a form if regulation requires one the user
    /// hasn't answered yet — and initializes the Google Mobile Ads SDK once
    /// <see cref="UMPConsentInformation.CanRequestAds" /> allows it. Safe to call from multiple
    /// <see cref="BannerAd" /> instances: the underlying request and initialization run once per
    /// process. Must be called on the main thread with the view controller hosting the Avalonia view.
    /// </summary>
    /// <param name="viewController">
    /// The view controller to present the consent form on, if one is required.
    /// </param>
    /// <returns>
    /// <see langword="true" /> once ads may be requested; <see langword="false" /> if consent is
    /// required and was not obtained.
    /// </returns>
    internal static Task<bool> EnsureReadyAsync(UIViewController viewController) =>
        _readyTask ??= RequestConsentAndInitializeAsync(viewController);

    /// <summary>
    /// Re-opens the privacy options form on the active scene's key window. A no-op that completes
    /// immediately when no view controller is available.
    /// </summary>
    /// <returns>
    /// A task that completes when the form is dismissed.
    /// </returns>
    internal static Task ShowPrivacyOptionsAsync()
    {
        var viewController = ResolveTopViewController();
        return viewController is null
            ? Task.CompletedTask
            : ShowPrivacyOptionsAsync(viewController);
    }

    /// <summary>
    /// Re-opens the privacy options form so the user can change a previously made consent choice.
    /// Google requires apps using UMP to offer this somewhere in their settings once real (not test)
    /// ads ship.
    /// </summary>
    /// <param name="viewController">
    /// The view controller to present the form on.
    /// </param>
    private static Task ShowPrivacyOptionsAsync(UIViewController viewController)
    {
        var tcs = new TaskCompletionSource();
        UMPConsentForm.PresentPrivacyOptionsForm(viewController, _ => tcs.TrySetResult());
        return tcs.Task;
    }

    private static UIViewController? ResolveTopViewController() =>
        UIApplication
            .SharedApplication.ConnectedScenes.OfType<UIWindowScene>()
            .SelectMany(scene => scene.Windows)
            .FirstOrDefault(window => window.IsKeyWindow)
            ?.RootViewController;

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "UMPRequestParameters is consumed by the native RequestConsentInfoUpdate call; disposing it here would break the callback."
    )]
    private static async Task<bool> RequestConsentAndInitializeAsync(
        UIViewController viewController
    )
    {
        var consentInformation = UMPConsentInformation.SharedInstance;

        var parameters = new UMPRequestParameters
        {
            TagForUnderAgeOfConsent = AdMobRuntime.Options.TagForUnderAgeOfConsent,
        };

        var updateTcs = new TaskCompletionSource();
        consentInformation.RequestConsentInfoUpdate(parameters, _ => updateTcs.TrySetResult());
        await updateTcs.Task;

        // Cache whether a privacy options entry point applies, so the About screen can offer a
        // "manage consent" control only to users a form was shown to (e.g. EEA under GDPR).
        _privacyOptionsRequired =
            consentInformation.PrivacyOptionsRequirementStatus
            == UMPPrivacyOptionsRequirementStatus.Required;

        // No-ops internally if no form is required (already obtained, or not required at all).
        var formTcs = new TaskCompletionSource();
        UMPConsentForm.LoadAndPresentIfRequired(viewController, _ => formTcs.TrySetResult());
        await formTcs.Task;

        if (!consentInformation.CanRequestAds)
        {
            return false;
        }

        if (AdMobRuntime.Options.TestDeviceIds.Count > 0)
        {
            GADMobileAds.SharedInstance.RequestConfiguration.TestDeviceIdentifiers =
                AdMobRuntime.Options.TestDeviceIds.ToArray();
        }

        GADMobileAds.SharedInstance.Start(null);
        return true;
    }
}
