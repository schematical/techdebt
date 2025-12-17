using UnityEngine;

public class InternetPipe : InfrastructureInstance
{
    private float timeSinceLastPacket = 0f;

    void Update()
    {
        // Ensure packet generation only runs during the Play phase and the pipe is operational
        if (
			GameManager.Instance.GameLoopManager.CurrentState == GameLoopManager.GameState.Play && 
			data.CurrentState == InfrastructureData.State.Operational
		) {
            float packetsPerSecond = GameManager.Instance.GetStat(StatType.Traffic);
            int connectionCount = data.NetworkConnections?.Count ?? 0;

            // Check if there are any configured network connections
            if (packetsPerSecond > 0 && connectionCount > 0)
            {
                timeSinceLastPacket += Time.deltaTime;
                float delay = 1f / packetsPerSecond;

                if (timeSinceLastPacket >= delay)
                {
                    timeSinceLastPacket -= delay;
                    NetworkPacketData data = GameManager.Instance.GetNetworkPacketData();
                    string targetId = GetNextNetworkTargetId(data.Type);
                    
                    if (targetId != null)
                    {
                        IDataReceiver targetReceiver = GameManager.Instance.GetReceiver(targetId);

                        if (targetReceiver is InfrastructureInstance destination)
                        {
                            // Create the packet
                            string fileName = $"file_{Random.Range(1000, 9999)}.dat";
                            int size = Random.Range(5, 50);
                            NetworkPacket packet = GameManager.Instance.CreatePacket(data, fileName, size, this);
                            
                            packet.SetNextTarget(destination);
                        }
                        else
                        {
                            Debug.LogWarning($"InternetPipe cannot create packet: Target receiver '{targetId}' not found in GameManager.");
                        }
                    }
                }
            }
            else
            {
                // Reset timer if traffic drops or no appliance is connected
                timeSinceLastPacket = 0f;
            }
        }
        else
        {
            // Reset timer when not in Play phase
            timeSinceLastPacket = 0f;
        }
    }

    // The InternetPipe itself doesn't "receive" packets in the traditional sense, it only generates them.
    // We override the base method to prevent it from trying to forward packets it receives.
    public override void ReceivePacket(NetworkPacket packet)
    {
        
        // Destroy the packet as it has finished its journey.
        GameManager.Instance.DestroyPacket(packet);
    }
}
