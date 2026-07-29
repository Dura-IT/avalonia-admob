using System.Collections.ObjectModel;
using Avalonia.Threading;

namespace DuraIT.Avalonia.AdMob.Sample.Logging;

/// <summary>
/// Collects log messages for display in the sample's on-screen readout. Appends are marshalled onto
/// the UI thread because the ad SDK's load and presentation callbacks may fire off it.
/// </summary>
public sealed class UiLogSink
{
    private const int MaxMessages = 200;

    /// <summary>
    /// Gets the log messages, newest last, bound to the UI.
    /// </summary>
    public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

    /// <summary>
    /// Appends a message to the on-screen log, trimming the oldest entries past the cap.
    /// </summary>
    public void Append(string message) =>
        Dispatcher.UIThread.Post(() =>
        {
            Messages.Add(message);
            while (Messages.Count > MaxMessages)
            {
                Messages.RemoveAt(0);
            }
        });
}
