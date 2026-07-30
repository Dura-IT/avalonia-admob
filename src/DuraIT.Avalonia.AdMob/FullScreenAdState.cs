namespace DuraIT.Avalonia.AdMob;

/// <summary>
/// The lifecycle state of a full-screen ad (interstitial, rewarded, app open) tracked by
/// <see cref="FullScreenAdController" />.
/// </summary>
internal enum FullScreenAdState
{
    /// <summary>
    /// No ad is loaded and none is loading.
    /// </summary>
    Idle,

    /// <summary>
    /// A load has been requested and the SDK's load callback has not returned yet.
    /// </summary>
    Loading,

    /// <summary>
    /// An ad is loaded and ready to present.
    /// </summary>
    Loaded,

    /// <summary>
    /// A loaded ad is currently presented full-screen.
    /// </summary>
    Showing,
}
