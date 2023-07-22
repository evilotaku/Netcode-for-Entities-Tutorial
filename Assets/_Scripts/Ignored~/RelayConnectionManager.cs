using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct JoinCode : IComponentData
{
    public FixedString64Bytes Value;
}

public class RelayConnectionManager : ConnectionManager
{    
    RelayHost m_Host;
    RelayClient m_Client;

    enum State { Unknown, SetupHost, SetupClient, JoinByCode, JoinLocalHost, Connected}

    State state = State.Unknown;

    // Start is called before the first frame update
    void Start()
    {
        state = State.SetupHost;
    }   

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.SetupHost:
            {
                    print(state);
                StartHostSystem();
                state = State.SetupClient;
                break;
            }               
            case State.SetupClient:
            {
                    print(state);
                    var isHostLocal = m_Host?.RelayData.Endpoint.IsValid;
                if(isHostLocal.GetValueOrDefault())
                {
                    StartClientSystem();
                    m_Client.GetJoinCodeFromHost();
                    state = State.JoinLocalHost;
                }                
                break;
            }
            case State.JoinByCode:
            {
                    print(state);
                    var hasClientConnectedToRelayService = m_Client?.ClientData.Endpoint.IsValid;
                if (hasClientConnectedToRelayService.GetValueOrDefault())
                {
                    ConnectToRelayServer();
                    state = State.Connected;
                }                
                break;
            }
            case State.JoinLocalHost:
            {
                    print(state);
                    var isClientSetup = m_Client?.ClientData.Endpoint.IsValid;
                if (isClientSetup.GetValueOrDefault())
                {
                    SetupRelayHostedServerAndConnect();
                    state = State.Connected;
                }                
                break;
            }
            case State.Connected:
                {
                    print(state);
                    state = State.Unknown;
                    break;
                }
            case State.Unknown:
                break;
        }
    }

    [ContextMenu("Start Relay Host")]
    void StartHostSystem()
    {
        var world = World.All[0];
        m_Host = world.GetOrCreateSystemManaged<RelayHost>();
        var enableRelay = world.EntityManager.CreateEntity(ComponentType.ReadWrite<EnableRelay>());
        world.EntityManager.AddComponent<EnableRelay>(enableRelay);

        var simGroup = world.GetExistingSystemManaged<SimulationSystemGroup>();
        simGroup.AddSystemToUpdateList(m_Host);
    }

    void StartClientSystem()
    {
        var world = World.All[0];
        m_Client = world.GetOrCreateSystemManaged<RelayClient>();
        var simGroup = world.GetExistingSystemManaged<SimulationSystemGroup>();
        simGroup.AddSystemToUpdateList(m_Client);
    }

    void SetupRelayHostedServerAndConnect()
    {
        if (ClientServerBootstrap.RequestedPlayType != ClientServerBootstrap.PlayType.ClientAndServer)
        {
            UnityEngine.Debug.LogError($"Creating client/server worlds is not allowed if playmode is set to {ClientServerBootstrap.RequestedPlayType}");
            return;
        }

        var world = World.All[0];
        var relayClientData = world.GetExistingSystemManaged<RelayClient>().ClientData;
        var relayServerData = world.GetExistingSystemManaged<RelayHost>().RelayData;
        var joinCode = world.GetExistingSystemManaged<RelayHost>().joinCode;

        var oldConstructor = NetworkStreamReceiveSystem.DriverConstructor;
        NetworkStreamReceiveSystem.DriverConstructor = new RelayDriver(relayClientData, relayServerData);
        var server = ClientServerBootstrap.CreateServerWorld("ServerWorld");
        var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");
        NetworkStreamReceiveSystem.DriverConstructor = oldConstructor;

       
        DestroyLocalWorld();

        if (World.DefaultGameObjectInjectionWorld == null)
            World.DefaultGameObjectInjectionWorld = server;

        SceneManager.LoadSceneAsync("Game");

        var joinCodeEntity = server.EntityManager.CreateEntity(ComponentType.ReadOnly<JoinCode>());
        server.EntityManager.SetComponentData(joinCodeEntity, new JoinCode { Value = joinCode });

        var networkStreamEntity = server.EntityManager.CreateEntity(ComponentType.ReadWrite<NetworkStreamRequestListen>());
        server.EntityManager.SetName(networkStreamEntity, "NetworkStreamRequestListen");
        server.EntityManager.SetComponentData(networkStreamEntity, new NetworkStreamRequestListen { Endpoint = NetworkEndpoint.AnyIpv4.WithPort(Port) });

        networkStreamEntity = client.EntityManager.CreateEntity(ComponentType.ReadWrite<NetworkStreamRequestConnect>());
        client.EntityManager.SetName(networkStreamEntity, "NetworkStreamRequestConnect");

        client.EntityManager.SetComponentData(networkStreamEntity, new NetworkStreamRequestConnect { Endpoint = relayClientData.Endpoint });
        Debug.Log($"Connecting local client to: {relayClientData.Endpoint}");
    }

    void ConnectToRelayServer()
    {
        var world = World.All[0];
        var relayClientData = world.GetExistingSystemManaged<RelayClient>().ClientData;

        var oldConstructor = NetworkStreamReceiveSystem.DriverConstructor;
        NetworkStreamReceiveSystem.DriverConstructor = new RelayDriver(new RelayServerData(), relayClientData);
        var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");
        NetworkStreamReceiveSystem.DriverConstructor = oldConstructor;
                
        DestroyLocalWorld();
        if (World.DefaultGameObjectInjectionWorld == null)
            World.DefaultGameObjectInjectionWorld = client;

        SceneManager.LoadSceneAsync("Game");

        var networkStreamEntity = client.EntityManager.CreateEntity(ComponentType.ReadWrite<NetworkStreamRequestConnect>());
        client.EntityManager.SetName(networkStreamEntity, "NetworkStreamRequestConnect");

        client.EntityManager.SetComponentData(networkStreamEntity, new NetworkStreamRequestConnect { Endpoint = relayClientData.Endpoint });
    }
}
