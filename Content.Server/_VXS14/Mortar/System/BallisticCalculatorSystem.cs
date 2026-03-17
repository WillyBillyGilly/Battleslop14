using Content.Server.EUI;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared._VXS14.Mortar;
using Robust.Server.Player;
using Content.Shared.Verbs;

namespace Content.Server._VXS14.Mortar;

public sealed class BallisticCalculatorSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BallisticCalculatorComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<BallisticCalculatorComponent, GetVerbsEvent<ExamineVerb>>(OnGetExamineVerb);
    }

    private void OnUseInHand(EntityUid uid, BallisticCalculatorComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        OpenCalculator(uid, args.User);
        args.Handled = true;
    }

    private void OnGetExamineVerb(EntityUid uid, BallisticCalculatorComponent component, GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        args.Verbs.Add(new ExamineVerb
        {
            Text = "Open ballistic calculator",
            Act = () => OpenCalculator(uid, args.User),
        });
    }

    private void OpenCalculator(EntityUid calculatorUid, EntityUid user)
    {
        if (!_playerManager.TryGetSessionByEntity(user, out var session))
            return;

        var eui = IoCManager.Resolve<EuiManager>();
        eui.OpenEui(new BallisticCalculatorEui(calculatorUid, user), session);
    }
}
