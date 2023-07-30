using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class PlayerInputSystem : SystemBase
{
    PlayerControls input;
    PlayerControls.PlayerActions actions;
    bool hasFired;
    Vector2 _Look;

    protected override void OnCreate()
    {
        input = new();
        input.Enable();
        actions = input.Player;
        RequireForUpdate<PlayerInput>();
        RequireForUpdate<NetworkId>();
        actions.Look.performed += (ctx) =>
        {            
            _Look = ctx.ReadValue<Vector2>();
        };
    }

    protected override void OnUpdate()
    {        
        InputSystem.Update();
        foreach (var input in SystemAPI.Query<RefRW<PlayerInput>>()
            .WithAll<GhostOwnerIsLocal>())
        {
            input.ValueRW.Movement = default;
            input.ValueRW.PrimeFire = default;
            
            Vector2 movement = actions.Move.ReadValue<Vector2>();
            float fire = actions.Fire.ReadValue<float>();

            Vector3 direction = Camera.main.transform.rotation * Vector3.forward;
            direction = new Vector3(direction.x, 0, direction.z).normalized;
            Vector3 mov = Quaternion.FromToRotation(Vector3.forward, direction) * new Vector3(movement.x, 0, movement.y);
            input.ValueRW.Movement = new float2(mov.x, mov.z);

            input.ValueRW.Look.y = math.clamp(input.ValueRW.Look.y + _Look.y, -math.PI / 2, math.PI / 2);
            input.ValueRW.Look.x = math.fmod(input.ValueRW.Look.x + _Look.x, 2 * math.PI);
            if (fire != 0 && !hasFired)
            {
                input.ValueRW.PrimeFire.Set();
            }
            hasFired = fire != 0;
            _Look = Vector2.zero;
        };  
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


