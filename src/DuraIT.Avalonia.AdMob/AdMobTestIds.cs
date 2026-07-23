namespace DuraIT.Avalonia.AdMob;

/// <summary>
///     Google's public sample ad unit ids, used when test ads are enabled. They serve real-looking test
///     ads that never generate impressions or revenue. Verify against
///     <c>developers.google.com/admob/android/test-ads</c> (and the iOS equivalent) before relying on them.
/// </summary>
internal static class AdMobTestIds
{
#if ANDROID
    /// <summary>
    /// The sample banner ad unit id for the current platform.
    /// </summary>
    internal const string Banner = "ca-app-pub-3940256099942544/6300978111";
#elif IOS
    /// <summary>
    ///     The sample banner ad unit id for the current platform.
    /// </summary>
    internal const string Banner = "ca-app-pub-3940256099942544/2934735716";
#else
    /// <summary>
    /// The sample banner ad unit id for the current platform.
    /// </summary>
    internal const string Banner = "ca-app-pub-3940256099942544/6300978111";
#endif
}
