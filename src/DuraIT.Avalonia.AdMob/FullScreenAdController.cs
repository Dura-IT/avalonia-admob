using System;
using Microsoft.Extensions.Logging;

namespace DuraIT.Avalonia.AdMob;

/// <summary>
/// Platform-agnostic lifecycle state machine for a full-screen ad (interstitial, rewarded, app open).
/// The per-platform service owns the native ad object and forwards its load/show callbacks into this
/// controller, which enforces the legal state transitions, tracks readiness (including optional
/// freshness expiry for app-open ads), and emits structured logs. Keeping the decision logic here —
/// rather than in the platform glue — is what makes the behaviour unit-testable.
/// </summary>
internal sealed class FullScreenAdController
{
    private readonly ILogger _logger;
    private readonly string _format;
    private readonly TimeSpan? _freshness;
    private readonly TimeProvider _timeProvider;

    private string _adUnitId = string.Empty;
    private DateTimeOffset? _loadedAt;

    /// <summary>
    /// Initializes a new instance of the <see cref="FullScreenAdController" /> class.
    /// </summary>
    /// <param name="logger">
    /// The logger that load and presentation outcomes are written to.
    /// </param>
    /// <param name="format">
    /// A short format label (for example <c>interstitial</c>) included in every log message.
    /// </param>
    /// <param name="freshness">
    /// How long a loaded ad stays showable before it is considered stale, or <see langword="null" />
    /// for formats that do not expire (interstitial, rewarded). App-open ads pass four hours.
    /// </param>
    /// <param name="timeProvider">
    /// The time source used to evaluate <paramref name="freshness" />.
    /// </param>
    public FullScreenAdController(
        ILogger logger,
        string format,
        TimeSpan? freshness,
        TimeProvider timeProvider
    )
    {
        _logger = logger;
        _format = format;
        _freshness = freshness;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Gets the current lifecycle state.
    /// </summary>
    public FullScreenAdState State { get; private set; } = FullScreenAdState.Idle;

    /// <summary>
    /// Gets a value indicating whether a loaded, unexpired ad is ready to present.
    /// </summary>
    public bool IsReady => State == FullScreenAdState.Loaded && !IsExpired;

    private bool IsExpired =>
        _freshness is { } freshness
        && _loadedAt is { } loadedAt
        && _timeProvider.GetUtcNow() - loadedAt > freshness;

    /// <summary>
    /// Transitions to <see cref="FullScreenAdState.Loading" /> when the controller is idle or already
    /// holds a loaded ad. Returns <see langword="false" /> — so the caller skips the native load —
    /// when a load or presentation is already in flight.
    /// </summary>
    /// <param name="adUnitId">
    /// The resolved ad unit id the load targets, recorded for subsequent log messages.
    /// </param>
    public bool TryBeginLoad(string adUnitId)
    {
        if (State is FullScreenAdState.Loading or FullScreenAdState.Showing)
        {
            return false;
        }

        _adUnitId = adUnitId;
        _loadedAt = null;
        State = FullScreenAdState.Loading;
        return true;
    }

    /// <summary>
    /// Records a successful native load: the ad becomes ready and its freshness clock starts.
    /// </summary>
    public void MarkLoaded()
    {
        _loadedAt = _timeProvider.GetUtcNow();
        State = FullScreenAdState.Loaded;
        FullScreenAdLog.Loaded(_logger, _format, _adUnitId);
    }

    /// <summary>
    /// Records a failed native load and returns the controller to
    /// <see cref="FullScreenAdState.Idle" />.
    /// </summary>
    public void MarkFailedToLoad(long errorCode, string errorMessage)
    {
        State = FullScreenAdState.Idle;
        _loadedAt = null;
        FullScreenAdLog.FailedToLoad(_logger, _format, _adUnitId, errorCode, errorMessage);
    }

    /// <summary>
    /// Transitions to <see cref="FullScreenAdState.Showing" /> when an ad is ready. Returns
    /// <see langword="false" /> — so the caller skips the native show — when no unexpired ad is loaded.
    /// </summary>
    public bool TryBeginShow()
    {
        if (!IsReady)
        {
            return false;
        }

        State = FullScreenAdState.Showing;
        return true;
    }

    /// <summary>
    /// Records that the ad was presented full-screen.
    /// </summary>
    public void MarkShown() => FullScreenAdLog.Showed(_logger, _format, _adUnitId);

    /// <summary>
    /// Records a failed presentation and returns the controller to
    /// <see cref="FullScreenAdState.Idle" />.
    /// </summary>
    public void MarkFailedToShow(long errorCode, string errorMessage)
    {
        State = FullScreenAdState.Idle;
        _loadedAt = null;
        FullScreenAdLog.FailedToShow(_logger, _format, _adUnitId, errorCode, errorMessage);
    }

    /// <summary>
    /// Records that the user dismissed the ad. A full-screen ad is single-use, so the controller
    /// returns to <see cref="FullScreenAdState.Idle" /> and the caller must load again before the
    /// next presentation.
    /// </summary>
    public void MarkDismissed()
    {
        State = FullScreenAdState.Idle;
        _loadedAt = null;
        FullScreenAdLog.Dismissed(_logger, _format, _adUnitId);
    }
}
