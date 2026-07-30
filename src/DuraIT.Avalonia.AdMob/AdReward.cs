namespace DuraIT.Avalonia.AdMob;

/// <summary>
/// The reward a user earned by watching a rewarded or rewarded interstitial ad to completion. The
/// amount and type are configured on the ad unit in the AdMob console — the SDK reports back whatever
/// was set there. Grant the reward only when your show call yields one; if the user dismisses the ad
/// early, none is earned.
/// </summary>
/// <param name="Type">
/// The reward type configured on the ad unit (for example <c>coins</c>), or an empty string when the
/// unit does not name one.
/// </param>
/// <param name="Amount">
/// The reward quantity configured on the ad unit.
/// </param>
public sealed record AdReward(string Type, decimal Amount);
