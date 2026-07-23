<p align="center">
  <img src="https://raw.githubusercontent.com/Dura-IT/avalonia-admob/main/assets/wordmark.png" alt="&lt;AdMob/&gt;" width="380" />
</p>

<h1 align="center">DuraIT.Avalonia.AdMob</h1>

<p align="center">
  A minimal, free, open-source <strong>AdMob banner-ad control for Avalonia</strong>.
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/DuraIT.Avalonia.AdMob"><img src="https://img.shields.io/nuget/v/DuraIT.Avalonia.AdMob.svg?logo=nuget" alt="NuGet" /></a>
  <a href="https://www.nuget.org/packages/DuraIT.Avalonia.AdMob"><img src="https://img.shields.io/nuget/dt/DuraIT.Avalonia.AdMob.svg?logo=nuget" alt="Downloads" /></a>
  <a href="./LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="MIT" /></a>
</p>

---

Avalonia ships no ad SDK, and AdMob/Meta/Unity provide MAUI plugins but nothing for Avalonia. This library hosts the
**native** AdMob banner — Android `AdView`, iOS `GADBannerView` — inside the Avalonia visual tree through a
`NativeControlHost`, so you drop one control into your XAML and get a real banner on both mobile heads.

It is deliberately **minimal**: a banner, done well. No interstitials, rewarded, native, or
mediation — [open an issue](https://github.com/Dura-IT/avalonia-admob/issues) if you need more.

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

### 1. Register the service

Call `AddAdMobBanner` once during startup, wherever you build your service collection:

```csharp
using DuraIT.Avalonia.AdMob;

services.AddAdMobBanner(options =>
{
    options.UseTestAds = true; // serve Google's sample test ads during development
});
```

Keep `UseTestAds = true` throughout development — it substitutes Google's public sample ad units, so no real impressions
or revenue are generated.

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
