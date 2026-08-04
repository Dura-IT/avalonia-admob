using Avalonia.Controls;
using Avalonia.Layout;

namespace DuraIT.Avalonia.AdMob.Platforms
{
    /// <summary>
    /// Desktop rendering of <see cref="BannerAd" />: a lightweight placeholder occupying the banner's
    /// footprint so desktop layouts match the mobile heads, without hosting any native ad view. The
    /// desktop head is a developer run/debug target and never serves real ads.
    /// </summary>
    public partial class BannerAd : Border
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BannerAd" /> class.
        /// </summary>
        public BannerAd()
        {
            Height = 50;
            string adUnitId =
                AdUnitResolver.Resolve(AdUnitId, null, AdMobTestIds.Banner)
                ?? "no ad unit configured";
            Child = new TextBlock
            {
                Text = $"AdMob banner — {adUnitId}",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = 0.4,
            };
        }
    }
}
