<p align="center">
  <img src="https://raw.githubusercontent.com/Dura-IT/avalonia-admob/main/assets/wordmark.png" alt="&lt;AdMob/&gt;" width="380" />
</p>

<h1 align="center">DuraIT.Avalonia.AdMob</h1>

<p align="center">
  Free, open-source <strong>AdMob ads for Avalonia</strong> — native banner control, interstitials, and rewarded ads.
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/DuraIT.Avalonia.AdMob"><img src="https://img.shields.io/nuget/v/DuraIT.Avalonia.AdMob.svg?logo=nuget" alt="NuGet" /></a>
  <a href="https://www.nuget.org/packages/DuraIT.Avalonia.AdMob"><img src="https://img.shields.io/nuget/dt/DuraIT.Avalonia.AdMob.svg?logo=nuget" alt="Downloads" /></a>
  <a href="https://sonarcloud.io/summary/overall?id=Dura-IT_avalonia-admob"><img src="https://sonarcloud.io/api/project_badges/measure?project=Dura-IT_avalonia-admob&metric=alert_status" alt="Quality Gate" /></a>
  <a href="https://sonarcloud.io/summary/overall?id=Dura-IT_avalonia-admob"><img src="https://sonarcloud.io/api/project_badges/measure?project=Dura-IT_avalonia-admob&metric=coverage" alt="Coverage" /></a>
  <a href="./LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="MIT" /></a>
</p>

---

Avalonia ships no ad SDK, and AdMob/Meta/Unity provide MAUI plugins but nothing for Avalonia. This library hosts the
**native** AdMob banner — Android `AdView`, iOS `GADBannerView` — inside the Avalonia visual tree through a
`NativeControlHost`, so you drop one control into your XAML and get a real banner on both mobile heads. Full-screen
**interstitial**, **rewarded**, and **rewarded interstitial** ads are supported too, presented by the native SDK on
demand.

More formats are on the way — app-open and native ads are
[tracked here](https://github.com/Dura-IT/avalonia-admob/issues).

## Platform support

| Platform | Target framework  | Renders                                                                    |
|----------|-------------------|----------------------------------------------------------------------------|
| Android  | `net10.0-android` | native `AdView`                                                            |
| iOS      | `net10.0-ios`     | native `GADBannerView`                                                     |
| Desktop  | `net10.0`         | inert placeholder strip (so shared UI compiles and runs on the debug head) |

## Install

```shell
dotnet add package DuraIT.Avalonia.AdMob
```

## Usage

### 1. Register the services

Call `AddAdMob` once during startup, wherever you build your service collection. It registers every ad format; inject
only the ones you use:

```csharp
using DuraIT.Avalonia.AdMob;

services.AddAdMob(options =>
{
    options.UseTestAds = true; // serve Google's sample test ads during development
});
```

Prefer to register a single format? Use `AddAdMobBanner`, `AddAdMobInterstitial`, `AddAdMobRewarded`, or
`AddAdMobRewardedInterstitial` instead — they take the same arguments.

Keep `UseTestAds = true` throughout development — it substitutes Google's public sample ad units, so no real impressions
or revenue are generated.

To validate a **real** ad unit before shipping without generating invalid traffic, register your device with Google
instead and list it in `TestDeviceIds` — real ad units then serve test creatives to that device only:

```csharp
services.AddAdMobBanner(options =>
{
    options.TestDeviceIds = ["33BE2250B43518CCDA7DE426D04EE231"]; // logged by the SDK on first run
});
```

### 2. Place the control

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:admob="using:DuraIT.Avalonia.AdMob.Platforms">
  <DockPanel>
    <admob:BannerAd DockPanel.Dock="Bottom" />
    <!-- your content -->
  </DockPanel>
</UserControl>
```

The control is a fixed 320×50 standard banner (`Height = 50`). With test ads enabled you can leave `AdUnitId` unset; for
production, set your real banner ad unit id:

```xml
<admob:BannerAd AdUnitId="ca-app-pub-XXXXXXXXXXXXXXXX/YYYYYYYYYY" />
```

### 3. Configure your app id per head

AdMob requires your **app id** in the platform manifest — even in test mode. Use the sample app ids below during
development and swap in your own for release.

**Android** — `AndroidManifest.xml`:

```xml
<application>
  <meta-data
    android:name="com.google.android.gms.ads.APPLICATION_ID"
    android:value="ca-app-pub-3940256099942544~3347511713" />
</application>
```

**iOS** — `Info.plist`:

```xml
<key>GADApplicationIdentifier</key>
<string>ca-app-pub-3940256099942544~1458002511</string>
<key>SKAdNetworkItems</key>
<array>
  <dict>
    <key>SKAdNetworkIdentifier</key>
    <string>cstr6suwn9.skadnetwork</string>
  </dict>
  <!-- Add Google's full SKAdNetwork buyer list before shipping. -->
</array>
```

That's it — the control loads and displays the banner. There is **no manual SDK-init call**: the Google Mobile Ads SDK
is initialized lazily, once consent allows it (see below).

## Interstitial ads

An interstitial is a full-screen ad the native SDK presents on demand — there is no control to place in XAML. Inject
`IInterstitialAdService`, load an ad ahead of the transition you want to interrupt, then present it at that point:

```csharp
using DuraIT.Avalonia.AdMob;

public sealed class GameOverViewModel
{
    private readonly IInterstitialAdService _interstitial;

    public GameOverViewModel(IInterstitialAdService interstitial) => _interstitial = interstitial;

    // Kick off the load early — e.g. when the level starts — so the ad is ready by the transition.
    public Task PreloadAsync() => _interstitial.LoadAsync();

    public async Task ShowGameOverAsync()
    {
        if (_interstitial.IsReady)
        {
            await _interstitial.ShowAsync();
        }

        // Load the next one — an interstitial is single-use.
        await _interstitial.LoadAsync();
    }
}
```

`LoadAsync` resolves consent first and only requests an ad once it is allowed; with test ads enabled you can leave the
ad unit unset, or pass your own: `LoadAsync("ca-app-pub-XXXXXXXXXXXXXXXX/YYYYYYYYYY")`. `ShowAsync` returns `false` when
no ad is ready or the platform (desktop) has no ads, so callers never need a platform check. The app-id manifest setup
above (step 3) is shared — an interstitial needs no extra platform configuration.

## Rewarded ads

A rewarded ad grants the user something in-app (coins, a hint, an extra life) in exchange for watching it to completion.
Inject `IRewardedAdService`, load ahead of time, and present it when the user opts in. Unlike an interstitial,
`ShowAsync` returns an `AdReward?` — the reward is earned only if the user finishes the ad, so grant it only when the
result is non-`null`:

```csharp
using DuraIT.Avalonia.AdMob;

public sealed class ShopViewModel
{
    private readonly IRewardedAdService _rewarded;

    public ShopViewModel(IRewardedAdService rewarded) => _rewarded = rewarded;

    // Preload so the "Watch for coins" button can enable itself the moment an ad is ready.
    public Task PreloadAsync() => _rewarded.LoadAsync();

    public async Task WatchForCoinsAsync()
    {
        if (_rewarded.IsReady)
        {
            AdReward? reward = await _rewarded.ShowAsync();
            if (reward is not null)
            {
                GrantCoins(reward.Amount); // reward.Type / reward.Amount come from the ad-unit config
            }
        }

        // Load the next one — a rewarded ad is single-use.
        await _rewarded.LoadAsync();
    }
}
```

**Rewarded interstitial** works identically — inject `IRewardedInterstitialAdService` instead. It shows at a natural
transition without the user opting in first, but still returns an `AdReward?` for watching to completion. Both formats
share the same app-id manifest setup (step 3) and need no extra platform configuration.

## Consent (GDPR / UMP)

For users in regulated regions (e.g. the EEA), the control requests consent through Google's **User Messaging Platform**
*before* any ad is requested, presents the consent form if one is required, and only then initializes the SDK — Google's
documented conditional-initialization pattern. If the consent service can't be reached, it fails open (a transient
network hiccup won't permanently block ads).

Once real ads ship, Google requires a persistent way for users to change their choice. `IBannerAdService` exposes it:

```csharp
public sealed class SettingsViewModel
{
    private readonly IBannerAdService _bannerAds;

    public SettingsViewModel(IBannerAdService bannerAds) => _bannerAds = bannerAds;

    // Show a "Manage ad consent" button only where a privacy-options entry point applies.
    public bool CanManageConsent => _bannerAds.IsPrivacyOptionsRequired;

    public Task ManageConsentAsync() => _bannerAds.ShowPrivacyOptionsAsync();
}
```

If your app targets users under the age of consent, set `options.TagForUnderAgeOfConsent = true`.

## Logging (optional)

Pass an `ILoggerFactory` to surface ad-load outcomes (loaded, failed, blocked-by-consent):

```csharp
services.AddAdMobBanner(
    options => options.UseTestAds = true,
    loggerFactory); // e.g. new SerilogLoggerFactory()
```

Without one, ad-load logging is silently discarded.

## Testing gotchas

- **Ad-blocking VPNs/DNS break test ads.** Proton VPN NetShield (and similar) return `NXDOMAIN` for Google's ad domains,
  so the request fails as an invalid request / no fill with a blank banner. Turn ad-blocking off while testing. The
  **iOS simulator uses your Mac's network stack**, so it inherits any host-level VPN/DNS block.
- **Test-ad throttling.** Hammering a test ad unit (≈10+ rapid loads) makes Google return "no fill" for a cooldown
  period. Not a bug — space out your requests.

## License

[MIT](./LICENSE) © Durable IT Solutions
