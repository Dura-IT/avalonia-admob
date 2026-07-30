using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DuraIT.Avalonia.AdMob;
using DuraIT.Avalonia.AdMob.Sample.Logging;

namespace DuraIT.Avalonia.AdMob.Sample.ViewModels;

/// <summary>
/// Drives the smoke-test screen: loads and shows each ad format on demand and surfaces the outcome
/// through <see cref="Status" /> and the shared on-screen <see cref="Log" />.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IInterstitialAdService _interstitial;
    private readonly IRewardedAdService _rewarded;
    private readonly IRewardedInterstitialAdService _rewardedInterstitial;

    public MainViewModel(
        IInterstitialAdService interstitial,
        IRewardedAdService rewarded,
        IRewardedInterstitialAdService rewardedInterstitial,
        UiLogSink logSink
    )
    {
        _interstitial = interstitial;
        _rewarded = rewarded;
        _rewardedInterstitial = rewardedInterstitial;
        Log = logSink.Messages;
        Status = _interstitial.IsSupported
            ? "Ready. Load a format, wait for the log to report it loaded, then show it."
            : "Full-screen ads are not supported on this platform (desktop) — banner shows a placeholder.";
    }

    /// <summary>
    /// Gets the shared on-screen log of ad-load and presentation outcomes.
    /// </summary>
    public ObservableCollection<string> Log { get; }

    /// <summary>
    /// Gets or sets the one-line status shown above the buttons.
    /// </summary>
    [ObservableProperty]
    public partial string Status { get; set; }

    [RelayCommand]
    private async Task LoadInterstitialAsync()
    {
        Status = "Interstitial: load requested — watch the log for the loaded event.";
        await _interstitial.LoadAsync();
        Status = _interstitial.IsReady
            ? "Interstitial: ready — press Show."
            : "Interstitial: loading… watch the log.";
    }

    [RelayCommand]
    private async Task ShowInterstitialAsync()
    {
        bool shown = await _interstitial.ShowAsync();
        Status = shown
            ? "Interstitial: shown — load another before showing again."
            : "Interstitial: nothing ready to show (load one first).";
    }

    [RelayCommand]
    private async Task LoadRewardedAsync()
    {
        Status = "Rewarded: load requested — watch the log for the loaded event.";
        await _rewarded.LoadAsync();
        Status = _rewarded.IsReady
            ? "Rewarded: ready — press Show and watch to completion to earn the reward."
            : "Rewarded: loading… watch the log.";
    }

    [RelayCommand]
    private async Task ShowRewardedAsync()
    {
        AdReward? reward = await _rewarded.ShowAsync();
        Status = DescribeRewardOutcome("Rewarded", reward);
    }

    [RelayCommand]
    private async Task LoadRewardedInterstitialAsync()
    {
        Status = "Rewarded interstitial: load requested — watch the log for the loaded event.";
        await _rewardedInterstitial.LoadAsync();
        Status = _rewardedInterstitial.IsReady
            ? "Rewarded interstitial: ready — press Show."
            : "Rewarded interstitial: loading… watch the log.";
    }

    [RelayCommand]
    private async Task ShowRewardedInterstitialAsync()
    {
        AdReward? reward = await _rewardedInterstitial.ShowAsync();
        Status = DescribeRewardOutcome("Rewarded interstitial", reward);
    }

    private static string DescribeRewardOutcome(string format, AdReward? reward) =>
        reward is not null
            ? $"{format}: earned {reward.Amount} {reward.Type} — load another before showing again."
            : $"{format}: dismissed without a reward (or nothing was ready to show).";
}
