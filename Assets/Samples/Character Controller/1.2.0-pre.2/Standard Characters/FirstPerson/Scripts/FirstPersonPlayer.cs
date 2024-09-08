using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct FirstPersonPlayer : IComponentData
{
    public Entity ControlledCharacter;
}

[Serializable]
public struct FirstPersonPlayerInputs : IComponentData
{
    public float2 MoveInput;
    public float2 LookInput;
    public FixedInputEvent JumpPressed;
}