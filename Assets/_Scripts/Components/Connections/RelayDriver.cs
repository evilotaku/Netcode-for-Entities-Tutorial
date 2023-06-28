using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport.Relay;
using UnityEngine;

public class RelayDriver : INetworkStreamDriverConstructor
{

    RelayServerData _clientData, _serverData;

    public RelayDriver(RelayServerData clientData, RelayServerData serverData)
    {
        _clientData = clientData;
        _serverData = serverData;
    }

    public void CreateClientDriver(World world, ref NetworkDriverStore driver, NetDebug netDebug)
    {
        var settings = DefaultDriverBuilder.GetNetworkSettings();
        settings.WithRelayParameters(ref _clientData);
        DefaultDriverBuilder.RegisterClientDriver(world, ref driver, netDebug, ref _clientData);
        //Forcing the local client to use the RelayService for testing 
        //DefaultDriverBuilder.RegisterClientUdpDriver(world, ref driver, netDebug, settings);
    }

    public void CreateServerDriver(World world, ref NetworkDriverStore driver, NetDebug netDebug)
    {
        DefaultDriverBuilder.RegisterServerDriver(world, ref driver, netDebug, ref _serverData);
    }    
}
