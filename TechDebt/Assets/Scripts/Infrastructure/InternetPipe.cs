using UnityEngine;

public class InternetPipe : InfrastructureInstance
{
   

    public void SendPacket(NetworkPacketData networkPacketData)
    {
        int connectionCount = data.NetworkConnections?.Count ?? 0;
        NetworkConnection connection = GetNextNetworkConnection(networkPacketData.Type);
        string targetId = connection?.TargetID;
                
        if (targetId != null)
        {
            InfrastructureInstance targetReceiver = GameManager.Instance.GetInfrastructureInstanceByID(targetId);

            if (targetReceiver is InfrastructureInstance destination)
            {
                // Create the packet
                string fileName = $"file_{Random.Range(1000, 9999)}.dat";
                int size = Random.Range(5, 50);
                NetworkPacket packet = GameManager.Instance.CreatePacket(networkPacketData, fileName, size, this);
                        
                packet.SetNextTarget(destination);
            }
            else
            {
                Debug.LogWarning($"InternetPipe cannot create packet: Target receiver '{targetId}' not found in GameManager.");
            }
        }
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
