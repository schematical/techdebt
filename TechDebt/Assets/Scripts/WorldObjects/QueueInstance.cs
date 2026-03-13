// QueueInstance.cs

using UnityEngine;

public class QueueInstance : InfrastructureInstance
{
    public NetworkPacketData batchJobPacketData;

    public override void Initialize()
    {
        base.Initialize();
        if (GameManager.Instance != null && GameManager.Instance.GetNetworkPacketDatas() != null)
        {
            batchJobPacketData = GameManager.Instance.GetNetworkPacketDatas()
                .Find(data => data.Type == NetworkPacketData.PType.BatchJob);
            if (batchJobPacketData == null)
            {
                Debug.LogWarning(
                    $"QueueInstance '{data.Id}': Could not find BatchJob NetworkPacketData in GameManager.");
            }
        }
    }

    protected override void RoutePacket(NetworkPacket packet)
    {
        // Custom logic for Queue: if it's a Text packet, transform it.
        if (!packet.IsReturning())
        {
            // 1. Create and send a new BatchJob packet forward.
            if (batchJobPacketData != null)
            {
                NetworkConnection connection = GetNextNetworkConnection(NetworkPacketData.PType.BatchJob);
                if (connection != null)
                {
                    InfrastructureInstance nextTarget =
                        GameManager.Instance.GetRandomWorldObjectByType(connection.worldObjectType);
                    if (nextTarget != null && nextTarget.IsActive())
                    {
                        NetworkPacket batchPacket =
                            GameManager.Instance.CreatePacket(batchJobPacketData, "batch.dat", 100, this);
                        batchPacket.SetNextTarget(nextTarget);
                        batchPacket.MoveToNextNode(); // Move the new packet immediately
                    }
                    else
                    {
                        Debug.LogError("No nextTarget found");
                    }
                }
                else
                {
                    Debug.LogError($"{gameObject.name} - No connections found");
                }
            }

            // 2. The original Text packet is not forwarded, so it starts its return journey.
            packet.StartReturn();
        }
        else
        {
            // For all other packet types (or returning packets), use the default routing behavior.
            base.RoutePacket(packet);
        }
    }
}