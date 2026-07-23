using System.Threading.Tasks;

namespace DuraIT.Avalonia.AdMob.Platforms;

/// <summary>
/// Android implementation of <see cref="IBannerAdService" />.
/// </summary>
internal sealed class BannerAdService : IBannerAdService
{
    public bool IsSupported => true;

    public bool IsPrivacyOptionsRequired => AndroidBannerAds.IsPrivacyOptionsRequired;

    public Task ShowPrivacyOptionsAsync() => AndroidBannerAds.ShowPrivacyOptionsAsync();
}
