using System.Threading.Tasks;

namespace DuraIT.Avalonia.AdMob.Platforms;

/// <summary>
/// iOS implementation of <see cref="IBannerAdService" />.
/// </summary>
internal sealed class BannerAdService : IBannerAdService
{
    public bool IsSupported => true;

    public bool IsPrivacyOptionsRequired => IosBannerAds.IsPrivacyOptionsRequired;

    public Task ShowPrivacyOptionsAsync() => IosBannerAds.ShowPrivacyOptionsAsync();
}
