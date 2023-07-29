using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UI;

public class RollbackUI : MonoBehaviour
{
    public static bool EnableRollback = true;

    public Toggle RollbackToggle;

    void Update()
    {
        EnableRollback = RollbackToggle.isOn;
    }
}

public struct RollbackEnabled : IComponentData { }

public struct ToggleRollbackRequest : IRpcCommand
{
    public bool Enable;
    public Entity Player;
}



[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct RollbackUISystem : ISystem
{
    bool m_PrevEnabled;

    public void OnCreate(ref SystemState state)
    {        
        state.RequireForUpdate<NetworkId>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (m_PrevEnabled == RollbackUI.EnableRollback
            || !SystemAPI.TryGetSingletonEntity<PlayerInput>(out var player)) return;
        
        m_PrevEnabled = RollbackUI.EnableRollback;
        var ent = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentData(ent, new ToggleRollbackRequest { Enable = m_PrevEnabled, Player = player });
        state.EntityManager.AddComponentData(ent, default(SendRpcCommandRequest));
    }
}

[RequireMatchingQueriesForUpdate]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct LagUIControlSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Process requests to toggle lag compensation
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (toggleRO, entity) in SystemAPI.Query<RefRO<ToggleRollbackRequest>>().WithEntityAccess())
        {
            // Find the correct control entity
            var toggle = toggleRO.ValueRO;
            switch (toggle.Enable)
            {
                case false when state.EntityManager.HasComponent<RollbackEnabled>(toggle.Player):
                    cmdBuffer.RemoveComponent<RollbackEnabled>(toggle.Player);
                    break;
                case true when !state.EntityManager.HasComponent<RollbackEnabled>(toggle.Player):
                    cmdBuffer.AddComponent<RollbackEnabled>(toggle.Player);
                    break;
            }
            cmdBuffer.DestroyEntity(entity);
        }
        cmdBuffer.Playback(state.EntityManager);
    }
}
