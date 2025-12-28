using Content.Server.Atmos.EntitySystems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Atmos;
using Content.Shared.Explosion.Components;
using Content.Shared.Explosion.Components.OnTrigger;
using Content.Shared.Projectiles;
using Content.Shared.Projectiles.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Server._Battlefield14.Projectiles;

/// <summary>
/// System for handling HEAT (High Explosive Anti-Tank) projectiles and similar penetration-based projectiles.
/// Handles explosions, fragmentation, and plasma gas release after penetration stops.
/// </summary>
public sealed class HeatSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly TriggerSystem _triggerSystem = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly GunSystem _gunSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HeatComponent, TriggerEvent>(OnHeatTriggered);
    }

    private void OnHeatTriggered(EntityUid uid, HeatComponent component, TriggerEvent args)
    {
        // Handle delayed fragmentation (triggered by timer)
        if (component.FragmentationTriggered &&
            component.FragmentationPrototype != null &&
            component.FragmentationCount > 0)
        {
            FragmentProjectile(uid, component);
            // Mark fragmentation as completed to prevent re-triggering
            component.FragmentationTriggered = false;
            Dirty(uid, component);
        }

        // Handle delayed plasma release (triggered by timer)
        if (component.PlasmaReleaseTriggered && component.PlasmaAmount > 0)
        {
            ReleasePlasmaGas(uid, component);
            // Mark plasma release as completed to prevent re-triggering
            component.PlasmaReleaseTriggered = false;
            Dirty(uid, component);
        }

        // Handle delayed explosion - ExplodeOnTrigger component will handle this,
        // but we need to ensure ExplosiveComponent exists
        // (ExplodeOnTrigger component's handler will call TriggerExplosive)
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HeatComponent, ProjectileComponent>();
        while (query.MoveNext(out var uid, out var heat, out var projectile))
        {
            // Check if projectile penetration has stopped
            if (!projectile.ProjectileSpent || heat.ExplosionTriggered)
                continue;

            // Mark as processed to prevent multiple triggers
            // We use ExplosionTriggered as a general "penetration processed" flag since all HEAT projectiles
            // conceptually handle penetration, even if they don't all explode
            heat.ExplosionTriggered = true;
            Dirty(uid, heat);

            // Prevent deletion until after effects complete
            projectile.DeleteOnCollide = false;
            Dirty(uid, projectile);

            // Handle fragmentation (with delay if specified)
            if (heat.FragmentationPrototype != null && heat.FragmentationCount > 0)
            {
                if (heat.FragmentationDelay <= 0)
                {
                    // Instant fragmentation - execute immediately, don't touch flag
                    FragmentProjectile(uid, heat);
                }
                else
                {
                    // Delayed fragmentation - mark as pending and schedule timer
                    heat.FragmentationTriggered = true;
                    Dirty(uid, heat);
                    _triggerSystem.HandleTimerTrigger(uid, null, heat.FragmentationDelay, 0, null, null);
                }
            }

            // Handle plasma gas release (with delay if specified)
            if (heat.PlasmaAmount > 0)
            {
                if (heat.PlasmaReleaseDelay <= 0)
                {
                    // Instant plasma release - execute immediately, don't touch flag
                    ReleasePlasmaGas(uid, heat);
                }
                else
                {
                    // Delayed plasma release - mark as pending and schedule timer
                    heat.PlasmaReleaseTriggered = true;
                    Dirty(uid, heat);
                    _triggerSystem.HandleTimerTrigger(uid, null, heat.PlasmaReleaseDelay, 0, null, null);
                }
            }

            // Handle explosion (with delay if specified)
            // Note: Instant explosion queues entity deletion, but other instant effects above
            // will have already executed in the same frame before deletion occurs
            if (heat.ExplosionDelay <= 0)
            {
                // Instant explosion - trigger directly
                if (TryComp<ExplosiveComponent>(uid, out var explosive))
                {
                    _explosionSystem.TriggerExplosive(uid, explosive, delete: true);
                }
            }
            else
            {
                // Delayed explosion - requires ExplodeOnTrigger component to handle the trigger event
                // The ExplodeOnTrigger component's event handler will call TriggerExplosive
                if (!HasComp<ExplodeOnTriggerComponent>(uid))
                {
                    Log.Error($"HEAT projectile {uid} has ExplosionDelay > 0 but is missing ExplodeOnTriggerComponent. Explosion will not trigger.");
                }
                _triggerSystem.HandleTimerTrigger(uid, null, heat.ExplosionDelay, 0, null, null);
            }
        }
    }

    private void FragmentProjectile(EntityUid uid, HeatComponent component)
    {
        var projectileCoord = _transformSystem.GetMapCoordinates(uid);
        var segmentAngle = 360f / component.FragmentationCount;

        for (int i = 0; i < component.FragmentationCount; i++)
        {
            var angleMin = segmentAngle * i;
            var angleMax = segmentAngle * (i + 1);
            var angle = Angle.FromDegrees(_random.NextFloat(angleMin, angleMax));
            var direction = angle.ToVec().Normalized();
            var velocity = _random.NextVector2(5f, 10f);

            var fragUid = Spawn(component.FragmentationPrototype, projectileCoord);
            _gunSystem.ShootProjectile(fragUid, direction, velocity, uid, null);
        }
    }

    private void ReleasePlasmaGas(EntityUid uid, HeatComponent component)
    {
        var environment = _atmosphereSystem.GetContainingMixture(uid, false, true);
        if (environment == null)
            return;

        // Create plasma gas mixture with high temperature
        var plasmaMixture = new GasMixture(1) { Temperature = component.PlasmaTemperature };
        plasmaMixture.SetMoles(Gas.Plasma, component.PlasmaAmount);

        // Merge into environment - the high temperature will rapidly heat the area
        _atmosphereSystem.Merge(environment, plasmaMixture);
    }
}


