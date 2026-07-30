using System.Threading.Tasks;

namespace DuraIT.Avalonia.AdMob.Platforms;

/// <summary>
/// Desktop implementation of <see cref="IRewardedInterstitialAdService" />. Ads never render on
/// desktop, so this reports <see cref="IRewardedInterstitialAdService.IsSupported" /> as
/// <see langword="false" /> and load/show are inert.
/// </summary>
internal sealed class RewardedInterstitialAdService : IRewardedInterstitialAdService
{
    public bool IsSupported => false;

    public bool IsReady => false;

    public Task LoadAsync(string? adUnitId = null) => Task.CompletedTask;

    public Task<AdReward?> ShowAsync() => Task.FromResult<AdReward?>(null);
}
