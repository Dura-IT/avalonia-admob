![<AdMob/>](https://raw.githubusercontent.com/Dura-IT/avalonia-admob/main/assets/wordmark.png)

# DuraIT.Avalonia.AdMob

Free, open-source **AdMob ads for Avalonia** — banner and native in-feed controls, plus interstitial, rewarded, and app-open ads.

[![NuGet](https://img.shields.io/nuget/v/DuraIT.Avalonia.AdMob.svg?logo=nuget)](https://www.nuget.org/packages/DuraIT.Avalonia.AdMob)
[![Downloads](https://img.shields.io/nuget/dt/DuraIT.Avalonia.AdMob.svg?logo=nuget)](https://www.nuget.org/packages/DuraIT.Avalonia.AdMob)
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=Dura-IT_avalonia-admob&metric=alert_status)](https://sonarcloud.io/summary/overall?id=Dura-IT_avalonia-admob)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Dura-IT_avalonia-admob&metric=coverage)](https://sonarcloud.io/summary/overall?id=Dura-IT_avalonia-admob)
[![MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/Dura-IT/avalonia-admob/blob/main/LICENSE)

---

Avalonia ships no ad SDK, and AdMob/Meta/Unity provide MAUI plugins but nothing for Avalonia. This library hosts the
**native** AdMob banner — Android `AdView`, iOS `GADBannerView` — inside the Avalonia visual tree through a
`NativeControlHost`, so you drop one control into your XAML and get a real banner on both mobile heads. A
**native (in-feed)** ad control renders inline the same way, blending into your own content. Full-screen
**interstitial**, **rewarded**, **rewarded interstitial**, and **app-open** ads are supported too, presented by the
native SDK on demand.

Found a bug or want a format that isn't here yet?
[Open an issue](https://github.com/Dura-IT/avalonia-admob/issues).

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

Prefer to register a single format? Use `AddAdMobBanner`, `AddAdMobInterstitial`, `AddAdMobRewarded`,
`AddAdMobRewardedInterstitial`, or `AddAdMobAppOpen` instead — they take the same arguments.

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

## App-open ads

An app-open ad is the full-screen ad shown while your app is loading or returning to the foreground. Inject
`IAppOpenAdService`, preload one, and present it from your own foreground hook. Two things set it apart from an
interstitial:

- **A loaded ad expires after four hours.** `IsReady` turns `false` once it goes stale, so check it (or just reload)
  before showing.
- **The library never shows it for you.** It presents on demand only — it does not subscribe to platform lifecycle
  events, so you decide *when* the foreground ad appears. This keeps the library out of your app's lifecycle and avoids
  showing an ad at the wrong moment (e.g. returning from your own consent dialog or an external payment sheet).

```csharp
using DuraIT.Avalonia.AdMob;

public sealed class AppOpenAdCoordinator
{
    private readonly IAppOpenAdService _appOpen;

    public AppOpenAdCoordinator(IAppOpenAdService appOpen) => _appOpen = appOpen;

    // Call once at startup, then again after each show, so an ad is always warming up.
    public Task PreloadAsync() => _appOpen.LoadAsync();

    // Wire this to your app's "resumed from background" event.
    public async Task OnResumedAsync()
    {
        if (_appOpen.IsReady)
        {
            await _appOpen.ShowAsync();
        }

        // Load the next one — the ad is single-use, and a fresh load resets the four-hour clock.
        await _appOpen.LoadAsync();
    }
}
```

It shares the same app-id manifest setup (step 3) and needs no extra platform configuration.

## Native ads

A native (in-feed) ad is one you place inline in your own layout — a list, a feed, between content cards — instead of a
fixed banner strip. Like the banner, it is a control you drop into XAML:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:admob="using:DuraIT.Avalonia.AdMob.Platforms">
  <StackPanel>
    <!-- your content -->
    <admob:NativeAd CornerRadius="8" Padding="12" />
    <!-- more content -->
  </StackPanel>
</UserControl>
```

With test ads enabled you can leave `AdUnitId` unset; for production, set your real native ad unit id:
`<admob:NativeAd AdUnitId="ca-app-pub-XXXXXXXXXXXXXXXX/YYYYYYYYYY" />`. Registration is the same as every other format —
`AddAdMob` covers it, or use `AddAdMobNative` if native is the only format you use. It shares the app-id manifest setup
(step 3) and needs no extra platform configuration.

### Why you style it with properties, not a template

Unlike an ordinary Avalonia control, a native ad's assets (headline, icon, media, call-to-action, etc.) **cannot be
composed in your own XAML template.** AdMob only counts an impression or click — and stays policy-compliant — when each
asset is a real native view registered with the platform SDK's asset wrapper (`NativeAdView` on Android,
`GADNativeAdView` on iOS). The control therefore renders a fixed native template internally and exposes styling through
properties instead. Styles are read **once**, when the native view is created; changing a style property after the ad
has rendered does not restyle it live.

| Property | Type | Purpose |
|---|---|---|
| `AdUnitId` | `string?` | Native ad unit to load (substituted with a sample unit when test ads are on). |
| `ShowIcon`, `ShowMedia`, `ShowBody`, `ShowAdvertiser`, `ShowStarRating`, `ShowPrice`, `ShowStore` | `bool` | Whether each optional asset is shown when the ad provides one. All default to `true`. |
| `CardBackground` | `Color?` | Card background color. `null` (default) keeps the platform's own default. |
| `HeadlineForeground`, `BodyForeground` | `Color?` | Text colors. `null` (default) keeps the platform defaults. |
| `CallToActionBackground`, `CallToActionForeground` | `Color?` | Call-to-action button colors. `null` (default) keeps the platform defaults. |
| `HeadlineFontSize`, `BodyFontSize` | `double` | Text sizes in device-independent pixels. Default `16` / `14`. |
| `CornerRadius` | `double` | Card corner radius. Default `0`. |
| `Padding` | `Thickness` | Padding between the card edge and its assets. Default none. |

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

[MIT](https://github.com/Dura-IT/avalonia-admob/blob/main/LICENSE) © Durable IT Solutions
