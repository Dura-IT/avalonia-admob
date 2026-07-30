using System.Threading.Tasks;

namespace DuraIT.Avalonia.AdMob;

/// <summary>
/// Platform-facing entry point for AdMob rewarded ads — a full-screen ad the user opts into and
/// watches to completion in exchange for an in-app reward. One implementation is registered per
/// platform head: a live implementation on Android and iOS, and an inert placeholder on desktop so
/// shared code can depend on the service unconditionally. Load an ad ahead of time, then present it
/// when the user chooses to earn the reward.
/// </summary>
public interface IRewardedAdService
{
    /// <summary>
    /// Gets a value indicating whether rewarded ads can be shown on the current platform. Returns
    /// <see langword="false" /> on desktop, where ads never render.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Gets a value indicating whether an ad has finished loading and is ready to present with
    /// <see cref="ShowAsync" />. Always <see langword="false" /> on desktop.
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// Begins loading a rewarded ad so it is ready to present later. Consent (UMP) is resolved first;
    /// if the user has not consented, no ad is requested. A no-op on desktop.
    /// </summary>
    /// <param name="adUnitId">
    /// The rewarded ad unit id to load, or <see langword="null" /> to use Google's sample test unit.
    /// Substituted with the sample unit whenever <see cref="AdMobOptions.UseTestAds" /> is enabled.
    /// </param>
    /// <returns>
    /// A task that completes once the load request has been issued.
    /// </returns>
    Task LoadAsync(string? adUnitId = null);

    /// <summary>
    /// Presents a previously loaded rewarded ad and waits for it to be dismissed. A rewarded ad is
    /// single-use, so load another with <see cref="LoadAsync" /> after it is dismissed.
    /// </summary>
    /// <returns>
    /// The <see cref="AdReward" /> the user earned by watching to completion, or <see langword="null" />
    /// if the user dismissed the ad early, no ad was ready, or the platform does not support rewarded
    /// ads. Grant the reward only when this is non-<see langword="null" />.
    /// </returns>
    Task<AdReward?> ShowAsync();
}
