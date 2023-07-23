using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviour
{

    public string Address;
    public ushort Port;
    public string Scene;
   

    // Start is called before the first frame update
    void Start()
    {
        StartHost();
    }

    [ContextMenu("Start Host")]
    public void StartHost()
    {       
        var server = ClientServerBootstrap.CreateServerWorld("ServerWorld");
        var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");
                
        DestroyLocalWorld();
        World.DefaultGameObjectInjectionWorld ??= server;

        SceneManager.LoadSceneAsync(Scene);
        {
            using var drvQuery = server.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(ClientServerBootstrap.DefaultListenAddress.WithPort(Port));

            //var listenRequest = server.EntityManager.CreateEntity(typeof(NetworkStreamRequestListen));
            //server.EntityManager.SetComponentData(listenRequest, new NetworkStreamRequestListen { Endpoint = ClientServerBootstrap.DefaultListenAddress.WithPort(Port) });
        }
       
        {
            using var drvQuery = client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(client.EntityManager, ClientServerBootstrap.DefaultConnectAddress.WithPort(Port));

            //var connectRequest = client.EntityManager.CreateEntity(typeof(NetworkStreamRequestConnect));
            //client.EntityManager.SetComponentData(connectRequest, new NetworkStreamRequestConnect { Endpoint = ClientServerBootstrap.DefaultConnectAddress.WithPort(Port) });
        }

        
    }

    [ContextMenu("Start Client")]
    public void StartClient()
    {
        var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");

        DestroyLocalWorld();
        World.DefaultGameObjectInjectionWorld ??= client;
        SceneManager.LoadSceneAsync(Scene);
        using var drvQuery = client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
        drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(client.EntityManager, NetworkEndpoint.Parse(Address,Port));


    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void DestroyLocalWorld()
    {
        foreach (var world in World.All)
        {
            if(world.Flags == WorldFlags.Game)
            {
                world.Dispose();
                break;
            }
        }
    }
}
