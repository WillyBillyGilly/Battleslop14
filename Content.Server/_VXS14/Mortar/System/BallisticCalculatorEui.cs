using Content.Server.EUI;
using Content.Shared.Eui;
using Content.Shared._VXS14.Mortar;
using JetBrains.Annotations;

namespace Content.Server._VXS14.Mortar;

[UsedImplicitly]
public sealed class BallisticCalculatorEui : BaseEui
{
    private readonly EntityUid _calculator;
    private readonly EntityUid _observer;

    public BallisticCalculatorEui(EntityUid calculator, EntityUid observer)
    {
        _calculator = calculator;
        _observer = observer;
    }

    public override void Opened()
    {
        base.Opened();
        SendSnapshot();
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not MortarBallisticCalculatorEuiMsg.CalculateRequest request)
            return;

        var entMan = IoCManager.Resolve<IEntityManager>();
        if (!entMan.TryGetComponent<BallisticCalculatorComponent>(_calculator, out var calculatorComp))
        {
            SendMessage(new MortarBallisticCalculatorEuiMsg.Error("Calculator component not found."));
            return;
        }

        var observer = ResolveObserver(entMan);
        if (observer == null)
        {
            SendMessage(new MortarBallisticCalculatorEuiMsg.Error("Observer entity is not available."));
            return;
        }

        var transform = entMan.System<SharedTransformSystem>();
        var observerWorld = transform.GetWorldPosition(observer.Value);

        var targetX = observerWorld.X + request.DeviationX;
        var targetY = observerWorld.Y + request.DeviationY;

        var mortarDx = targetX - request.MortarX;
        var mortarDy = targetY - request.MortarY;
        var mortarToTargetDistance = MathF.Sqrt((mortarDx * mortarDx) + (mortarDy * mortarDy));

        var flightTime = mortarToTargetDistance * calculatorComp.SecondsPerTile;
        var expectedDeviation = MathF.Max(calculatorComp.MinDeviation, mortarToTargetDistance * calculatorComp.DeviationPerTile);

        SendMessage(new MortarBallisticCalculatorEuiMsg.Result(
            flightTime,
            expectedDeviation,
            mortarToTargetDistance,
            targetX,
            targetY));

        SendSnapshot();
    }

    private void SendSnapshot()
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        if (!entMan.HasComponent<BallisticCalculatorComponent>(_calculator))
            return;

        var observer = ResolveObserver(entMan);
        if (observer == null)
            return;

        var observerWorld = entMan.System<SharedTransformSystem>().GetWorldPosition(observer.Value);
        SendMessage(new MortarBallisticCalculatorEuiMsg.Snapshot(
            observerWorld.X,
            observerWorld.Y));
    }

    private EntityUid? ResolveObserver(IEntityManager entMan)
    {
        if (entMan.EntityExists(_observer))
            return _observer;

        if (Player.AttachedEntity is { Valid: true } attached)
            return attached;

        return null;
    }
}
