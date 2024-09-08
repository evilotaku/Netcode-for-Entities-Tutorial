using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.NetCode.LowLevel;

[WithAll(typeof(GhostSpawnQueue))]
[BurstCompile]
partial struct ClassificationJob : IJobEntity
{
    public SnapshotDataLookupHelper snapshotDataLookupHelper;
    public Entity spawnListEntity;
    public BufferLookup<PredictedGhostSpawn> PredictedSpawnListLookup;
    public ComponentLookup<GrenadeData> grenadeDataLookup;
    public int ghostType;

    public void Execute(DynamicBuffer<GhostSpawnBuffer> ghosts, DynamicBuffer<SnapshotDataBuffer> data)
    {
        var predictedSpawnList = PredictedSpawnListLookup[spawnListEntity];
        var snapshotDataLookup = snapshotDataLookupHelper.CreateSnapshotBufferLookup();

        for (int i = 0; i < ghosts.Length; ++i)
        {
            var newGhostSpawn = ghosts[i];
            if (newGhostSpawn.GhostType != ghostType)
                continue; // Not a Grenade

            if (newGhostSpawn.SpawnType != GhostSpawnBuffer.Type.Predicted || newGhostSpawn.PredictedSpawnEntity != Entity.Null)
                continue; 


            newGhostSpawn.HasClassifiedPredictedSpawn = true;

            for (int j = 0; j < predictedSpawnList.Length; ++j)
            {
                if (newGhostSpawn.GhostType == predictedSpawnList[j].ghostType)
                {
                    if (snapshotDataLookup.TryGetComponentDataFromSnapshotHistory(newGhostSpawn.GhostType, data, out GrenadeData grenadeData, i))
                    {
                        var spawnIdFromList = grenadeDataLookup[predictedSpawnList[j].entity].SpawnId;
                        if (grenadeData.SpawnId == spawnIdFromList)
                        {
                            newGhostSpawn.PredictedSpawnEntity = predictedSpawnList[j].entity;
                            predictedSpawnList[j] = predictedSpawnList[predictedSpawnList.Length - 1];
                            predictedSpawnList.RemoveAt(predictedSpawnList.Length - 1);
                            break;
                        }
                    }
                }
            }
            ghosts[i] = newGhostSpawn;
        }        
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(GhostSimulationSystemGroup))]
[UpdateAfter(typeof(GhostSpawnClassificationSystem))]
[CreateAfter(typeof(GhostCollectionSystem))]
[CreateAfter(typeof(GhostReceiveSystem))]
[BurstCompile]
public partial struct ClassificationSystem : ISystem
{

    SnapshotDataLookupHelper m_SnapshotDataLookupHelper;
    BufferLookup<PredictedGhostSpawn> m_PredictedGhostSpawnLookup;
    ComponentLookup<GrenadeData> m_GrenadeDataLookup;
    
    int m_GhostType;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_SnapshotDataLookupHelper = new SnapshotDataLookupHelper(ref state,
                SystemAPI.GetSingletonEntity<GhostCollection>(),
                SystemAPI.GetSingletonEntity<SpawnedGhostEntityMap>());
        m_PredictedGhostSpawnLookup = state.GetBufferLookup<PredictedGhostSpawn>();
        m_GrenadeDataLookup = state.GetComponentLookup<GrenadeData>();

        state.RequireForUpdate<GhostSpawnQueue>();
        state.RequireForUpdate<PredictedGhostSpawnList>();
        state.RequireForUpdate<NetworkId>();
        state.RequireForUpdate<GrenadePrefab>();
        m_GhostType = -1;
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (m_GhostType == -1)
        {
            var prefabEntity = SystemAPI.GetSingleton<GrenadePrefab>().Grenade;
            var collectionEntity = SystemAPI.GetSingletonEntity<GhostCollection>();
            var ghostPrefabTypes = state.EntityManager.GetBuffer<GhostCollectionPrefab>(collectionEntity);

            for (int i = 0; i < ghostPrefabTypes.Length; ++i)
            {
                if (ghostPrefabTypes[i].GhostPrefab == prefabEntity)
                {
                    m_GhostType = i;
                    break;
                }                
            }
        }

        m_SnapshotDataLookupHelper.Update(ref state);
        m_PredictedGhostSpawnLookup.Update(ref state);
        m_GrenadeDataLookup.Update(ref state);

        var classificationJob = new ClassificationJob
        {
            snapshotDataLookupHelper = m_SnapshotDataLookupHelper,
            spawnListEntity = SystemAPI.GetSingletonEntity<PredictedGhostSpawnList>(),
            PredictedSpawnListLookup = m_PredictedGhostSpawnLookup,
            grenadeDataLookup = m_GrenadeDataLookup,
            ghostType = m_GhostType
        };

        state.Dependency = classificationJob.Schedule(state.Dependency);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}