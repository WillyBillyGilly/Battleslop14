using Content.Shared.GameTicking.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.GameObjects;

namespace Content.Server.GameTicking;

public sealed class ShiftTimeLockSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Cancel the whole interaction attempt before it even reaches the target.
        SubscribeLocalEvent<InteractionAttemptEvent>(OnInteractionAttempt);
        SubscribeLocalEvent<ShiftTimeLockComponent, GettingInteractedWithAttemptEvent>(OnGettingInteractedWithAttempt);
    }

    private void OnInteractionAttempt(EntityUid uid, InteractionAttemptEvent args)
    {
        if (args.Cancelled || args.Target == null)
            return;

        if (!EntityManager.TryGetComponent<ShiftTimeLockComponent>(args.Target.Value, out var comp))
            return;

        var currentShift = _gameTicker.RoundDuration();
        if (currentShift >= comp.ShiftTime)
            return;

        _popup.PopupClient(Loc.GetString("shift-time-lock-blocked"), uid);
        args.Cancelled = true;
    }

    private void OnGettingInteractedWithAttempt(EntityUid uid, ShiftTimeLockComponent component, ref GettingInteractedWithAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        var currentShift = _gameTicker.RoundDuration();
        if (currentShift >= component.ShiftTime)
        {
            RemComp<ShiftTimeLockComponent>(uid);
            return;
        }

        // Safety: cancel if the user attempted to interact despite being blocked.
        _popup.PopupClient(Loc.GetString("shift-time-lock-blocked"), args.Uid);
        args.Cancelled = true;
    }
}

