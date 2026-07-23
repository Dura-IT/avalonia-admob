using System.Threading.Tasks;

namespace DuraIT.Avalonia.AdMob.Platforms;

/// <summary>
///     Desktop implementation of <see cref="IBannerAdService" />. Ads never render on desktop, so this
///     reports <see cref="IBannerAdService.IsSupported" /> as <see langword="false" /> and the privacy
///     options are inert.
/// </summary>
internal sealed class BannerAdService : IBannerAdService
{
    public bool IsSupported => false;

    public bool IsPrivacyOptionsRequired => false;

    public Task ShowPrivacyOptionsAsync() => Task.CompletedTask;
}
