using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;


[BurstCompile]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[UpdateAfter(typeof(ShootingSystem))]
[RequireMatchingQueriesForUpdate]
public partial struct ApplyHitMarkSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<HitScan>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        // Do not perform hit-scan when rolling back, only when simulating the latest tick
        if (!networkTime.IsFirstTimeFullyPredictingTick)
            return;

        var isServer = state.WorldUnmanaged.IsServer();
        foreach (var (hit, serverHitMarker, clientHitMarker) in SystemAPI.Query<RefRO<HitScan>, RefRW<ServerHitMarker>, RefRW<ClientHitMarker>>().WithAll<Simulate>())
        {
            if (hit.ValueRO.Target == Entity.Null)
            {
                continue;
            }

            if (isServer)
            {
                serverHitMarker.ValueRW.Entity = hit.ValueRO.Target;
                serverHitMarker.ValueRW.HitPoint = hit.ValueRO.HitPoint;
                serverHitMarker.ValueRW.ServerHitTick = hit.ValueRO.Tick;
            }
            else
            {
                clientHitMarker.ValueRW.Entity = hit.ValueRO.Target;
                clientHitMarker.ValueRW.HitPoint = hit.ValueRO.HitPoint;
                clientHitMarker.ValueRW.ClientHitTick = hit.ValueRO.Tick;
            }
        }
    }
}