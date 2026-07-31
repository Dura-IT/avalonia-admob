using Avalonia.Controls;
using Avalonia.Layout;

namespace DuraIT.Avalonia.AdMob.Platforms;

/// <summary>
/// Desktop rendering of <see cref="NativeAd" />: a lightweight placeholder occupying the ad's
/// footprint so desktop layouts match the mobile heads, without hosting any native ad view. The
/// desktop head is a developer run/debug target and never serves real ads.
/// </summary>
/// <remarks>
/// Derives from <see cref="Panel" /> rather than <see cref="Border" /> because <see cref="Border" />
/// (via <c>TemplatedControl</c>) already declares its own <c>CornerRadius</c>/<c>Padding</c> properties,
/// which would collide with the shared control's style properties of the same name.
/// </remarks>
public partial class NativeAd : Panel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NativeAd" /> class.
    /// </summary>
    public NativeAd()
    {
        Height = 120;
        Children.Add(
            new Border
            {
                Child = new TextBlock
                {
                    Text = "AdMob native ad",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Opacity = 0.4,
                },
            }
        );
    }
}
