using System.Threading.Tasks;

namespace DuraIT.Avalonia.AdMob.Platforms;

/// <summary>
/// Desktop implementation of <see cref="IAppOpenAdService" />. Ads never render on desktop, so this
/// reports <see cref="IAppOpenAdService.IsSupported" /> as <see langword="false" /> and load/show are
/// inert.
/// </summary>
internal sealed class AppOpenAdService : IAppOpenAdService
{
    public bool IsSupported => false;

    public bool IsReady => false;

    public Task LoadAsync(string? adUnitId = null) => Task.CompletedTask;

    public Task<bool> ShowAsync() => Task.FromResult(false);
}
