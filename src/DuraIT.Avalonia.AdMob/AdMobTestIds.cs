namespace DuraIT.Avalonia.AdMob;

/// <summary>
/// Google's public sample ad unit ids, used when test ads are enabled. They serve real-looking test
/// ads that never generate impressions or revenue. Verify against
/// <c>developers.google.com/admob/android/test-ads</c> (and the iOS equivalent) before relying on them.
/// </summary>
internal static class AdMobTestIds
{
#if ANDROID
    /// <summary>
    /// The sample banner ad unit id for the current platform.
    /// </summary>
    internal const string Banner = "ca-app-pub-3940256099942544/6300978111";

    /// <summary>
    /// The sample interstitial ad unit id for the current platform.
    /// </summary>
    internal const string Interstitial = "ca-app-pub-3940256099942544/1033173712";

    /// <summary>
    /// The sample rewarded ad unit id for the current platform.
    /// </summary>
    internal const string Rewarded = "ca-app-pub-3940256099942544/5224354917";

    /// <summary>
    /// The sample rewarded interstitial ad unit id for the current platform.
    /// </summary>
    internal const string RewardedInterstitial = "ca-app-pub-3940256099942544/5354046379";
#elif IOS
    /// <summary>
    /// The sample banner ad unit id for the current platform.
    /// </summary>
    internal const string Banner = "ca-app-pub-3940256099942544/2934735716";

    /// <summary>
    /// The sample interstitial ad unit id for the current platform.
    /// </summary>
    internal const string Interstitial = "ca-app-pub-3940256099942544/4411468910";

    /// <summary>
    /// The sample rewarded ad unit id for the current platform.
    /// </summary>
    internal const string Rewarded = "ca-app-pub-3940256099942544/1712485313";

    /// <summary>
    /// The sample rewarded interstitial ad unit id for the current platform.
    /// </summary>
    internal const string RewardedInterstitial = "ca-app-pub-3940256099942544/6978759866";
#else
    /// <summary>
    /// The sample banner ad unit id for the current platform.
    /// </summary>
    internal const string Banner = "ca-app-pub-3940256099942544/6300978111";

    /// <summary>
    /// The sample interstitial ad unit id for the current platform.
    /// </summary>
    internal const string Interstitial = "ca-app-pub-3940256099942544/1033173712";

    /// <summary>
    /// The sample rewarded ad unit id for the current platform.
    /// </summary>
    internal const string Rewarded = "ca-app-pub-3940256099942544/5224354917";

    /// <summary>
    /// The sample rewarded interstitial ad unit id for the current platform.
    /// </summary>
    internal const string RewardedInterstitial = "ca-app-pub-3940256099942544/5354046379";
#endif
}
