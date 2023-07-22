using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(GhostInputSystemGroup))]
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
        RequireForUpdate<NetworkId>();
    }

    protected override void OnUpdate()
    {
        if (!SystemAPI.TryGetSingletonEntity<PlayerInput>(out var localInputEntity)) return;

        InputSystem.Update();

        var input = default(PlayerInput);
        input.Tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        input.movement = actions.Move.ReadValue<Vector2>();
        input.look = actions.Look.ReadValue<Vector2>();
        if (actions.Fire.ReadValue<float>() != 0)
            input.fire.Set();

        var buffer = EntityManager.GetBuffer<PlayerInput>(localInputEntity);
        buffer.AddCommandData(input);
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


