using UnityEngine;

public class InternetPipe : InfrastructureInstance
{
   

    public void SendPacket(NetworkPacketData networkPacketData)
    {
        // int connectionCount = data.NetworkConnections?.Count ?? 0;
        NetworkConnection connection = GetNextNetworkConnection(networkPacketData.Type);

                
   
        InfrastructureInstance targetReceiver = GameManager.Instance.GetRandomWorldObjectByType(connection.worldObjectType);

        
        // Create the packet
        string fileName = $"file_{networkPacketData.Type}_{Random.Range(1000, 9999)}.dat";
        int size = Random.Range(5, 50);
        NetworkPacket packet = GameManager.Instance.CreatePacket(networkPacketData, fileName, size, this);
                
        packet.SetNextTarget(targetReceiver);
       
        
    }

    // The InternetPipe itself doesn't "receive" packets in the traditional sense, it only generates them.
    // A packet arriving here has finished its journey. We override this method to destroy it.
    protected override bool HandleIncomingPacket(NetworkPacket packet)
    {
        // Destroy the packet as it has finished its journey.
        GameManager.Instance.DestroyPacket(packet);
        // Return false to prevent RoutePacket() and MoveToNextNode() from being called.
        return false;
    }
}
