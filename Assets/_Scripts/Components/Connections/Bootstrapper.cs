using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.NetCode;
using Unity.Entities;
using Unity.Networking.Transport;

[UnityEngine.Scripting.Preserve]
public class GameBootStrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        //Use 0 to manually connect
        AutoConnectPort = 0;

        if (AutoConnectPort != 0)
        {            
            return base.Initialize(defaultWorldName);
        }
        else
        {
            AutoConnectPort = 0;
            CreateLocalWorld(defaultWorldName);
            return true; ;
        }
    }
}

