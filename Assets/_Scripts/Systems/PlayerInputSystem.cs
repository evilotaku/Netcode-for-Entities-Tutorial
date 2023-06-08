using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class PlayerInputSystem : SystemBase
{
    SimpleControls input;
    SimpleControls.GameplayActions actions;

    protected override void OnCreate()
    {
        input = new();
        input.Enable();
        actions = input.gameplay;
    } 
   

    protected override void OnUpdate()
    {
        InputSystem.Update();
        Vector2 movement = actions.move.ReadValue<Vector2>();

        foreach (var playerInput in SystemAPI.Query<RefRW<PlayerInput>>()
            .WithAll<GhostOwnerIsLocal>())
        {
            playerInput.ValueRW.movement = movement;
        }
    }
}
