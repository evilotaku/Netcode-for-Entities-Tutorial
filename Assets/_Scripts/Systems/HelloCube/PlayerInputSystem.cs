using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[AlwaysSynchronizeSystem]
public partial class PlayerInputSystem : SystemBase
{
    DefaultInputActions input;
    DefaultInputActions.PlayerActions actions;

    protected override void OnCreate()
    {
        input = new();
        input.Enable();
        actions = input.Player;
        RequireForUpdate<PlayerInput>();
        RequireForUpdate<NetworkStreamInGame>();
    }

    protected override void OnUpdate()
    {
        InputSystem.Update();
        Vector2 movement = actions.Move.ReadValue<Vector2>();
        Vector2 look = actions.Look.ReadValue<Vector2>();
        bool fire = actions.Fire.ReadValue<float>() != 0;

        foreach (var playerInput in SystemAPI.Query<RefRW<PlayerInput>>()
            .WithAll<GhostOwnerIsLocal>())
        {
            playerInput.ValueRW = default;
            playerInput.ValueRW.movement = movement;
            playerInput.ValueRW.look = look;
            if (fire)
            {
                Debug.Log("Mouse Click!");
                playerInput.ValueRW.fire.Set();
            }
        }
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        input.Disable();
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


