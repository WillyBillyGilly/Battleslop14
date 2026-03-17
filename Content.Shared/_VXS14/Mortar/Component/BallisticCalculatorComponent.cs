using Robust.Shared.GameObjects;

namespace Content.Shared._VXS14.Mortar;

[RegisterComponent]
public sealed partial class BallisticCalculatorComponent : Component
{
    [DataField("secondsPerTile")]
    public float SecondsPerTile = 0.1f;

    [DataField("deviationPerTile")]
    public float DeviationPerTile = 0.03f;

    [DataField("minDeviation")]
    public float MinDeviation = 0.15f;
}
