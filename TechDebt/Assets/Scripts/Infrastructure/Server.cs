// Server.cs

using Stats;
using Unity.VisualScripting;
using UnityEngine;

public class Server : InfrastructureInstance
{
    // This class will hold the state of a server, such as whether it's ON or OFF,
    // its current Tech Debt level, and if it's on fire.
    public void Initialize()
    {
        data.Stats.Add(new StatData(StatType.Infra_DailyCost, 30));
        data.Stats.Add(new StatData(StatType.Infra_BuildTime, 10));
        data.Stats.Add(new StatData(StatType.Infra_LoadPerPacket, 20));
        data.Stats.Add(new StatData(StatType.Infra_MaxLoad, 100));
        data.Stats.Add(new StatData(StatType.Infra_LoadRecoveryRate, 50));
        data.Stats.Add(new StatData(StatType.TechDebt, 0f));

        NetworkConnection db = new NetworkConnection("dedicated-db", NetworkPacketData.PType.Text);
        data.NetworkConnections.Add(db);
    }
}
