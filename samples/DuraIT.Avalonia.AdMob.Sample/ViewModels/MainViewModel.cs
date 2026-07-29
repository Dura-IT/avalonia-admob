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

    public MainViewModel(IInterstitialAdService interstitial, UiLogSink logSink)
    {
        _interstitial = interstitial;
        Log = logSink.Messages;
        Status = _interstitial.IsSupported
            ? "Ready. Load an interstitial, wait for the log to report it loaded, then show it."
            : "Interstitial ads are not supported on this platform (desktop) — banner shows a placeholder.";
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
}
