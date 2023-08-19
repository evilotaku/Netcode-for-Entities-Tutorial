using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[GhostComponentVariation(typeof(KinematicCharacterBody))]
[GhostComponent]
public struct KinematicCharacterBodyGhostVariant 
{
    [GhostField] public bool IsGrounded;
    [GhostField(Quantization = 1000)] public float3 RelativeVelocity;
    [GhostField] public Entity ParentEntity;
    [GhostField(Quantization = 1000)] public float3 ParentLocalAnchorPoint;
    [GhostField(Quantization = 1000)] public float3 ParentVelocity;
}

[GhostComponentVariation(typeof(CharacterInterpolation))]
[GhostComponent(PrefabType = GhostPrefabType.PredictedClient)]
public struct CharacterInterpolationGhostVariant
{ }