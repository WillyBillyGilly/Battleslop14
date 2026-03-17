using Content.Client.Eui;
using Content.Shared.Eui;
using Content.Shared._VXS14.Mortar;
using JetBrains.Annotations;

namespace Content.Client._VXS14.Mortar;

[UsedImplicitly]
public sealed class BallisticCalculatorEui : BaseEui
{
    private readonly BallisticCalculatorWindow _window;

    public BallisticCalculatorEui()
    {
        _window = new BallisticCalculatorWindow();
        _window.SetEui(this);
        _window.OnClose += SendClosedMessage;
    }

    public override void Opened()
    {
        base.Opened();
        _window.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();
        _window.OnClose -= SendClosedMessage;
        _window.Close();
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        switch (msg)
        {
            case MortarBallisticCalculatorEuiMsg.Snapshot snapshot:
                _window.UpdateSnapshot(snapshot);
                break;
            case MortarBallisticCalculatorEuiMsg.Result result:
                _window.UpdateResult(result);
                break;
            case MortarBallisticCalculatorEuiMsg.Error error:
                _window.ShowError(error.Message);
                break;
        }
    }

    public void SendCalculate(float mortarX, float mortarY, float deviationX, float deviationY)
    {
        SendMessage(new MortarBallisticCalculatorEuiMsg.CalculateRequest(mortarX, mortarY, deviationX, deviationY));
    }

    private void SendClosedMessage()
    {
        SendMessage(new CloseEuiMessage());
    }
}
