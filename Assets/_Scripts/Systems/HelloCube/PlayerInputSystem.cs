using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
//[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial class PlayerInputSystem : SystemBase
{
    DefaultInputActions input;
    DefaultInputActions.PlayerActions actions;

    protected override void OnCreate()
    {
        input = new();
        input.Enable();
        actions = input.Player;
    }


    protected override void OnUpdate()
    {
        InputSystem.Update();
        Vector2 movement = actions.Move.ReadValue<Vector2>();
        Vector2 look = actions.Look.ReadValue<Vector2>();
        float fire = actions.Fire.ReadValue<float>();

        foreach (var (playerInput, entity) in SystemAPI.Query<RefRW<PlayerInput>>()
            .WithAll<GhostOwnerIsLocal>()
            .WithEntityAccess())
        {
            playerInput.ValueRW.movement = movement;
            playerInput.ValueRW.look = look;
            playerInput.ValueRW.fire = fire;            
        }

    }
}

/* if(Mouse.current.leftButton.wasPressedThisFrame)
 {
     Vector2 screenPos = Mouse.current.position.value;


     var entity = EntityManager.CreateEntity();
     EntityManager.AddBuffer<BuildingComponent>(entity);

     //UnityEngine.Ray ray = Camera.main.ScreenPointToRay(screenPos);
     *//*RaycastInput input = new RaycastInput
     {
         Start = ray.origin,
         Filter = CollisionFilter.Default,
         End = ray.GetPoint(Camera.main.farClipPlane)
     };*//*

     //EntityManager.GetBuffer<BuildingComponent>(entity).Add(new BuildingComponent { value = input, index = 0 });            

 }

 *//*PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
 DynamicBuffer<Buildings> buildings = SystemAPI.GetSingletonBuffer<Buildings>();
 var ecbBeginSim = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(EntityManager.WorldUnmanaged);

 foreach (var input in SystemAPI.Query<DynamicBuffer<BuildingComponent>>())
 {
     foreach(var place in input)
     {
         if(physicsWorld.CastRay(place.value, out var hit))
         {
             Debug.Log($"clicked on {hit.Position}");
             var entity = ecbBeginSim.Instantiate(buildings[place.index].Prefab);
             ecbBeginSim.SetComponent(entity, new LocalTransform
             {
                 Position = math.round(hit.Position) + math.up()
             });
         }
     }
     input.Clear();*/


