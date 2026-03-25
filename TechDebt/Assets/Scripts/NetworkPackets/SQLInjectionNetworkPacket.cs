using System;
using UnityEngine;
using Random = UnityEngine.Random;


namespace DefaultNamespace.NetworkPackets
{
    public class SQLInjectionNetworkPacket: NetworkPacket
    {
     
        public override void Initialize(NetworkPacketData npData, string fileName, int size,
            InfrastructureInstance origin = null)
        {
            base.Initialize(npData, fileName, size, origin);
       
        }
       
    }
}