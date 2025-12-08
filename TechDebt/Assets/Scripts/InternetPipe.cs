using UnityEngine;

public class InternetPipe : InfrastructureInstance
{
    private float timeSinceLastPacket = 0f;

    void Update()
    {
        // Ensure packet generation only runs during the Play phase and the pipe is operational
        if (GameManager.Instance.GameLoopManager.CurrentState == GameLoopManager.GameState.Play && data.CurrentState == InfrastructureData.State.Operational)
        {
            float packetsPerSecond = GameManager.Instance.GetStat(StatType.Traffic);
            int connectionCount = data.NetworkConnections?.Length ?? 0;

            // Check if there are any configured network connections
            if (packetsPerSecond > 0 && connectionCount > 0)
            {
                timeSinceLastPacket += Time.deltaTime;
                float delay = 1f / packetsPerSecond;

                if (timeSinceLastPacket >= delay)
                {
                    timeSinceLastPacket -= delay; // Subtract delay instead of resetting to 0 to handle high traffic rates more accurately

                    // Get the first configured connection ID
                    string targetId = data.NetworkConnections[0];
                    IDataReceiver targetReceiver = GameManager.Instance.GetReceiver(targetId);

                    if (targetReceiver != null)
                    {
                        // Create the packet
                        string fileName = $"file_{Random.Range(1000, 9999)}.dat";
                        int size = Random.Range(5, 50);
                        GameManager.Instance.CreatePacket(fileName, size, transform.position, targetReceiver);
                    }
                    else
                    {
                        Debug.LogWarning($"InternetPipe cannot create packet: Target receiver '{targetId}' not found in GameManager.");
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
        // Notify the GameManager that a packet has completed its round trip.
        GameManager.Instance.NotifyPacketRoundTripComplete();
        
        // Destroy the packet as it has finished its journey.
        GameManager.Instance.DestroyPacket(packet);
    }
}
