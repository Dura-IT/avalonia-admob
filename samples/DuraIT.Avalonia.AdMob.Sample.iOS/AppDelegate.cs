using Avalonia;
using Avalonia.iOS;
using DuraIT.Avalonia.AdMob.Sample;
using Foundation;

namespace DuraIT.Avalonia.AdMob.Sample.iOS;

[Register("AppDelegate")]
#pragma warning disable CA1711 // The "Delegate" suffix is required by the iOS runtime convention.
public partial class AppDelegate : AvaloniaAppDelegate<App>
#pragma warning restore CA1711
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder).WithInterFont();
}
