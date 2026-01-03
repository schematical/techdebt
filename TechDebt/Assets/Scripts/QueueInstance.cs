// QueueInstance.cs
using UnityEngine;

    public class QueueInstance : InfrastructureInstance
    {
        public NetworkPacketData batchJobPacketData;

        public override void Initialize()
        {
            base.Initialize();
            if (GameManager.Instance != null && GameManager.Instance.NetworkPacketDatas != null)
            {
                batchJobPacketData = GameManager.Instance.NetworkPacketDatas.Find(data => data.Type == NetworkPacketData.PType.BatchJob);
                if (batchJobPacketData == null)
                {
                    Debug.LogWarning($"QueueInstance '{data.DisplayName}': Could not find BatchJob NetworkPacketData in GameManager.");
                }
            }
        }

        protected override void RoutePacket(NetworkPacket packet)    {
        // Custom logic for Queue: if it's a Text packet, transform it.
        if (!packet.IsReturning())
        {
            // 1. Create and send a new BatchJob packet forward.
            if (batchJobPacketData != null)
            {
                NetworkConnection connection = GetNextNetworkConnection(NetworkPacketData.PType.BatchJob);
                if (connection != null)
                {
                    InfrastructureInstance nextTarget = GameManager.Instance.GetInfrastructureInstanceByID(connection.TargetID);
                    if (nextTarget != null && nextTarget.IsActive())
                    {
                        NetworkPacket batchPacket = GameManager.Instance.CreatePacket(batchJobPacketData, "batch.dat", 100, this);
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
                    Debug.LogError("No connections found");
                }
            }

            // 2. The original Text packet is not forwarded, so it starts its return journey.
            packet.StartReturn();
        }
        else
        {
            // For all other packet types (or returning packets), use the default routing behavior.
            Debug.Log("Routing Base Packet");
            base.RoutePacket(packet);
        }
    }
}

