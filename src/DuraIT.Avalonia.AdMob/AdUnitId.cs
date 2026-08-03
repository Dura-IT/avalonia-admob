namespace DuraIT.Avalonia.AdMob
{
    /// <summary>
    /// A platform-specific pair of AdMob ad unit ids. AdMob issues a distinct ad unit id per platform for
    /// the same logical placement, so a cross-platform app configures both: the Android id is used when the
    /// app runs on Android, the iOS id when it runs on iOS.
    /// </summary>
    /// <param name="Android">
    /// The ad unit id to use on Android, or <see langword="null" /> if the app does not target Android.
    /// </param>
    /// <param name="IOS">
    /// The ad unit id to use on iOS, or <see langword="null" /> if the app does not target iOS.
    /// </param>
    public sealed record AdUnitId(string? Android, string? IOS);
}
