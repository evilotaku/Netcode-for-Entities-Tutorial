using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.CharacterController;
using Unity.Physics.Authoring;

[Serializable]
public struct ThirdPersonCharacterComponent : IComponentData
{
    public float RotationSharpness;
    public float GroundMaxSpeed;
    public float GroundedMovementSharpness;
    public float AirAcceleration;
    public float AirMaxSpeed;
    public float AirDrag;
    public float JumpSpeed;
    public float3 Gravity;
    public bool PreventAirAccelerationAgainstUngroundedHits;
    public BasicStepAndSlopeHandlingParameters StepAndSlopeHandling;
    public int MaxAirJumps;
    [HideInInspector]
    public int CurrentAirJumps;
    public float SprintSpeedMultiplier;
    public CustomPhysicsBodyTags IgnoredPhysicsTags;

    public static ThirdPersonCharacterComponent GetDefault()
    {
        return new ThirdPersonCharacterComponent
        {
            RotationSharpness = 25f,
            GroundMaxSpeed = 10f,
            GroundedMovementSharpness = 15f,
            AirAcceleration = 50f,
            AirMaxSpeed = 10f,
            AirDrag = 0f,
            JumpSpeed = 10f,
            Gravity = math.up() * -30f,
            PreventAirAccelerationAgainstUngroundedHits = true,
            StepAndSlopeHandling = BasicStepAndSlopeHandlingParameters.GetDefault(),
        };
    }
}

[Serializable]
public struct ThirdPersonCharacterControl : IComponentData
{
    public float3 MoveVector;
    public bool Jump;
    public bool Sprint;
    public bool Fire;
}
