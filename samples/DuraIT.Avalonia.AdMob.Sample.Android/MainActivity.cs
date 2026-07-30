using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace DuraIT.Avalonia.AdMob.Sample.Android;

[Activity(
    Label = "AdMob Sample",
    Theme = "@style/MyTheme.NoActionBar",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation
        | ConfigChanges.ScreenSize
        | ConfigChanges.UiMode
)]
public class MainActivity : AvaloniaMainActivity { }
