using System.Threading.Tasks;

namespace DuraIT.Avalonia.AdMob.Platforms
{
    /// <summary>
    /// Desktop implementation of <see cref="IRewardedAdService" />. Ads never render on desktop, so this
    /// reports <see cref="IRewardedAdService.IsSupported" /> as <see langword="false" /> and load/show are
    /// inert.
    /// </summary>
    internal sealed class RewardedAdService : IRewardedAdService
    {
        public bool IsSupported => false;

        public bool IsReady => false;

        public Task LoadAsync(string? adUnitId = null) => Task.CompletedTask;

        public Task<AdReward?> ShowAsync() => Task.FromResult<AdReward?>(null);
    }
}
