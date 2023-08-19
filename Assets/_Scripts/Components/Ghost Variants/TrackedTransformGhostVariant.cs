using Unity.CharacterController;
using Unity.Mathematics;
using Unity.NetCode;

[GhostComponentVariation(typeof(TrackedTransform))]
[GhostComponent]
public struct TrackedTransformGhostVariant 
{
    [GhostField] public RigidTransform CurrentFixedRateTransform;
}