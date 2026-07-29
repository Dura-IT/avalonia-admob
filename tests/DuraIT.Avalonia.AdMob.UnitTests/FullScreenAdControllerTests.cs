using System;
using AwesomeAssertions;
using NUnit.Framework;

namespace DuraIT.Avalonia.AdMob.UnitTests;

[TestFixture]
[TestOf(typeof(FullScreenAdController))]
public class FullScreenAdControllerTests
{
    private const string AdUnit = "ca-app-pub-123/interstitial";

    private static FullScreenAdController CreateController(
        CapturingLogger logger,
        TimeSpan? freshness = null,
        TimeProvider? timeProvider = null
    ) =>
        new FullScreenAdController(
            logger,
            "interstitial",
            freshness,
            timeProvider ?? TimeProvider.System
        );

    [Test]
    public void NewController_IsIdleAndNotReady()
    {
        var controller = CreateController(new CapturingLogger());

        controller.State.Should().Be(FullScreenAdState.Idle);
        controller.IsReady.Should().BeFalse();
    }

    [Test]
    public void TryBeginLoad_WhenIdle_EntersLoadingAndReturnsTrue()
    {
        var controller = CreateController(new CapturingLogger());

        var started = controller.TryBeginLoad(AdUnit);

        started.Should().BeTrue();
        controller.State.Should().Be(FullScreenAdState.Loading);
    }

    [Test]
    public void TryBeginLoad_WhileLoading_ReturnsFalse()
    {
        var controller = CreateController(new CapturingLogger());
        controller.TryBeginLoad(AdUnit);

        var second = controller.TryBeginLoad(AdUnit);

        second.Should().BeFalse();
    }

    [Test]
    public void TryBeginLoad_WhileShowing_ReturnsFalse()
    {
        var controller = CreateController(new CapturingLogger());
        controller.TryBeginLoad(AdUnit);
        controller.MarkLoaded();
        controller.TryBeginShow();

        var reload = controller.TryBeginLoad(AdUnit);

        reload.Should().BeFalse();
    }

    [Test]
    public void TryBeginLoad_WhenLoaded_AllowsReload()
    {
        var controller = CreateController(new CapturingLogger());
        controller.TryBeginLoad(AdUnit);
        controller.MarkLoaded();

        var reload = controller.TryBeginLoad(AdUnit);

        reload.Should().BeTrue();
        controller.State.Should().Be(FullScreenAdState.Loading);
    }

    [Test]
    public void MarkLoaded_MakesReadyAndLogs()
    {
        var logger = new CapturingLogger();
        var controller = CreateController(logger);
        controller.TryBeginLoad(AdUnit);

        controller.MarkLoaded();

        controller.State.Should().Be(FullScreenAdState.Loaded);
        controller.IsReady.Should().BeTrue();
        logger.Entries.Should().ContainSingle();
    }

    [Test]
    public void MarkFailedToLoad_ReturnsToIdleAndLogsWarning()
    {
        var logger = new CapturingLogger();
        var controller = CreateController(logger);
        controller.TryBeginLoad(AdUnit);

        controller.MarkFailedToLoad(3, "No fill");

        controller.State.Should().Be(FullScreenAdState.Idle);
        controller.IsReady.Should().BeFalse();
        logger.Entries[^1].Message.Should().Contain("No fill");
    }

    [Test]
    public void TryBeginShow_WhenLoaded_EntersShowingAndReturnsTrue()
    {
        var controller = CreateController(new CapturingLogger());
        controller.TryBeginLoad(AdUnit);
        controller.MarkLoaded();

        var shown = controller.TryBeginShow();

        shown.Should().BeTrue();
        controller.State.Should().Be(FullScreenAdState.Showing);
    }

    [Test]
    public void TryBeginShow_WhenNotLoaded_ReturnsFalse()
    {
        var controller = CreateController(new CapturingLogger());

        var shown = controller.TryBeginShow();

        shown.Should().BeFalse();
        controller.State.Should().Be(FullScreenAdState.Idle);
    }

    [Test]
    public void MarkFailedToShow_ReturnsToIdleAndLogsWarning()
    {
        var logger = new CapturingLogger();
        var controller = CreateController(logger);
        controller.TryBeginLoad(AdUnit);
        controller.MarkLoaded();
        controller.TryBeginShow();

        controller.MarkFailedToShow(7, "Already used");

        controller.State.Should().Be(FullScreenAdState.Idle);
        logger.Entries[^1].Message.Should().Contain("Already used");
    }

    [Test]
    public void MarkDismissed_ReturnsToIdleAndLogs()
    {
        var logger = new CapturingLogger();
        var controller = CreateController(logger);
        controller.TryBeginLoad(AdUnit);
        controller.MarkLoaded();
        controller.TryBeginShow();
        controller.MarkShown();

        controller.MarkDismissed();

        controller.State.Should().Be(FullScreenAdState.Idle);
        controller.IsReady.Should().BeFalse();
        logger.Entries[^1].Message.Should().Contain("dismissed");
    }

    [Test]
    public void IsReady_WhenLoadedButExpired_IsFalse()
    {
        var time = new MutableTimeProvider(DateTimeOffset.UnixEpoch);
        var controller = CreateController(new CapturingLogger(), TimeSpan.FromHours(4), time);
        controller.TryBeginLoad(AdUnit);
        controller.MarkLoaded();

        time.Advance(TimeSpan.FromHours(5));

        controller.IsReady.Should().BeFalse();
        controller.TryBeginShow().Should().BeFalse();
    }

    [Test]
    public void IsReady_WhenLoadedAndWithinFreshness_IsTrue()
    {
        var time = new MutableTimeProvider(DateTimeOffset.UnixEpoch);
        var controller = CreateController(new CapturingLogger(), TimeSpan.FromHours(4), time);
        controller.TryBeginLoad(AdUnit);
        controller.MarkLoaded();

        time.Advance(TimeSpan.FromHours(3));

        controller.IsReady.Should().BeTrue();
    }
}

// A TimeProvider whose clock only moves when the test advances it, so freshness expiry is deterministic.
file sealed class MutableTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    public MutableTimeProvider(DateTimeOffset start) => _utcNow = start;

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public void Advance(TimeSpan delta) => _utcNow += delta;
}
