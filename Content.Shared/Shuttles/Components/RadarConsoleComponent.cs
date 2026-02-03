using Content.Shared.Shuttles.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Shuttles.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
// [Access(typeof(SharedRadarConsoleSystem))] // BF14
public sealed partial class RadarConsoleComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public float RangeVV
    {
        get => MaxRange;
        set => IoCManager
            .Resolve<IEntitySystemManager>()
            .GetEntitySystem<SharedRadarConsoleSystem>()
            .SetRange(Owner, value, this);
    }

    [DataField, AutoNetworkedField]
    public float MaxRange = 3072f; // Mono - 256->3072

    /// <summary>
    /// If true, the radar will be centered on the entity. If not - on the grid on which it is located.
    /// </summary>
    [DataField]
    public bool FollowEntity = false;

    // Frontier: ghost radar restrictions
    /// <summary>
    /// If true, the radar will be centered on the entity. If not - on the grid on which it is located.
    /// </summary>
    [DataField]
    public float? MaxIffRange = null;

    /// <summary>
    /// If true, the radar will not show the coordinates of objects on hover
    /// </summary>
    [DataField]
    public bool HideCoords = false;
    // End Frontier

    // <Mono>
    [DataField]
    public bool Pannable = true;

    [DataField]
    public bool RelativePanning = false;
    // </Mono>

    /// <summary>
    /// BF14 - whether sonar is available to this radar.
    /// </summary>
    [DataField]
    public bool HasSonar = false;

    /// <summary>
    /// BF14 - the coverage arc of this radar's sonar.
    /// </summary>
    [DataField]
    public Angle SonarWidth = Angle.FromDegrees(30);

    /// <summary>
    /// BF14 - how far can this radar's sonar scan.
    /// </summary>
    [DataField]
    public float SonarDistance = 800f;

    /// <summary>
    /// BF14 - for how long do sonar scan from this reveal grids.
    /// </summary>
    [DataField]
    public TimeSpan SonarDuration = TimeSpan.FromSeconds(7);

    /// <summary>
    /// BF14 - for how long does the sonar on this recharge.
    /// </summary>
    [DataField]
    public TimeSpan SonarCooldown = TimeSpan.FromSeconds(10);

    /// <summary>
    /// BF14 - when we last pulsed sonar. Only updated on client.
    /// </summary>
    [ViewVariables]
    public TimeSpan? SonarLastPulse = null;
}
