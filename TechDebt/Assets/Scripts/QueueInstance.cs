// QueueInstance.cs
using UnityEngine;

public class QueueInstance : InfrastructureInstance
{
    public NetworkPacketData batchJobPacketData;
    
    public override void ReceivePacket(NetworkPacket packet)
    {
        // =================================================================================
        // This is a manual reimplementation of the start of InfrastructureInstance.ReceivePacket
        // We cannot call base.ReceivePacket() because it calls MoveToNextNode() at the end,
        // which would cause a double-move when we have custom logic here.
        // =================================================================================

        if (data.CurrentState == InfrastructureData.State.Frozen)
        {
            packet.MarkFailed();
            packet.MoveToNextNode();
            return;
        }

        if (!packet.IsReturning())
        {
            // Simplified from base class - apply general load and cost for any packet received
            CurrentLoad += data.LoadPerPacket;
            GameManager.Instance.IncrStat(StatType.Money, data.CostPerPacket * -1);

            if (CurrentLoad > data.MaxLoad)
            {
                packet.MarkFailed();
                CurrentLoad = data.Stats.GetStatValue(StatType.Infra_MaxLoad);
                SetState(InfrastructureData.State.Frozen);
                packet.MoveToNextNode();
                return;
            }
        }
        
        // =================================================================================
        // Custom QueueInstance Logic
        // =================================================================================

        if (packet.data.Type == NetworkPacketData.PType.Text && !packet.IsReturning())
        {
            // 1. Transform Text packet into a BatchJob packet and send it forward.
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
                        batchPacket.MoveToNextNode();
                    }
                }
            }

            // 2. The original Text packet is not forwarded, so it starts its return journey.
            packet.StartReturn();
        }
        else
        {
            // If it's any other packet type (or a returning packet), forward it normally.
            NetworkConnection connection = GetNextNetworkConnection(packet.data.Type);
            if (connection != null)
            {
                InfrastructureInstance nextTarget = GameManager.Instance.GetInfrastructureInstanceByID(connection.TargetID);
                if (nextTarget != null && nextTarget.IsActive())
                {
                    packet.SetNextTarget(nextTarget);
                }
                else
                {
                    packet.StartReturn();
                }
            }
            else
            {
                packet.StartReturn();
            }
        }
        
        // Finally, move the original packet to its next node (either returning or forwarding).
        packet.MoveToNextNode();
    }
}
