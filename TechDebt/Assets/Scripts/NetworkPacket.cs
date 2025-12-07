// NetworkPacket.cs
using UnityEngine;

public class NetworkPacket : MonoBehaviour
{
    public string FileName { get; private set; }
    public int Size { get; private set; } // In MB for simplicity
    public Vector3 currentPosition; // Current position in world space
    public IDataReceiver nextHop; // The next destination for this packet

    public float speed = 2f;

    public void Initialize(string fileName, int size, Vector3 startPosition, IDataReceiver destination)
    {
        FileName = fileName;
        Size = size;
        currentPosition = startPosition;
        nextHop = destination;
        gameObject.name = $"Packet_{FileName}";
        transform.position = startPosition; // Set initial position of the GameObject
    }

    void Update()
    {
        if (nextHop == null)
        {
            // If there's no destination, destroy the packet to prevent clutter
            PacketManager.Instance.DestroyPacket(this);
            return;
        }

        Vector3 destinationPosition = nextHop.GetTransform().position;
        transform.position = Vector3.MoveTowards(transform.position, destinationPosition, speed * Time.deltaTime);

        // Check if the packet has reached its destination
        if (Vector3.Distance(transform.position, destinationPosition) < 0.1f)
        {
            // Deliver the packet
            nextHop.ReceivePacket(this);
            
            // For now, the packet's journey ends here.
            // In the future, the receiver might forward it.
            PacketManager.Instance.DestroyPacket(this);
        }
    }
}