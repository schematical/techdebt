// Server.cs
using UnityEngine;

public class Server : InfrastructureInstance
{
    // This class will hold the state of a server, such as whether it's ON or OFF,
    // its current Tech Debt level, and if it's on fire.

    public override void ReceivePacket(NetworkPacket packet)
    {
        base.ReceivePacket(packet);
        Debug.Log($"Server {data.DisplayName} is now processing file: {packet.FileName}");
        // Future logic for handling the packet can go here.
    }
}
