// IDataReceiver.cs
using UnityEngine;

// An interface to define any object that can receive a NetworkPacket
public interface IDataReceiver
{
    void ReceivePacket(NetworkPacket packet);
    Transform GetTransform(); // To know where to route packets visually
}
