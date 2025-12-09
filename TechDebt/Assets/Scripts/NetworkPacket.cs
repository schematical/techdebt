// NetworkPacket.cs
using UnityEngine;
using System.Collections.Generic;
using System.Data;

public class NetworkPacket : MonoBehaviour
{
    public string FileName { get; private set; }
    public int Size { get; private set; } // In MB for simplicity
    
    public bool isReturning = false;
    public Vector3 currentPosition; // Current position in world space
    public InfrastructureInstance nextHop; // The next destination for this packet
    
    public List<InfrastructureInstance> pastNodes = new List<InfrastructureInstance>();
    public float speed = 2f;


    public void Initialize(string fileName, int size, InfrastructureInstance origin = null)
    {
        FileName = fileName;
        Size = size;
        gameObject.name = $"Packet_{FileName}";
        pastNodes.Add(origin);
    }

    public void MoveToNextNode()
    {
        Debug.Log("Returning MoveToNextNode: " + isReturning); 
        if (isReturning)
        {
            
            nextHop = pastNodes[pastNodes.Count - 1];
            pastNodes.RemoveAt(pastNodes.Count - 1);
        }
    }

    public void SetNextTarget(InfrastructureInstance target)
    {
        nextHop = target;
        pastNodes.Add(nextHop);
    }
    void Update()
    {
        if (nextHop == null)
        {
            // If there's no destination, destroy the packet to prevent clutter
            GameManager.Instance.DestroyPacket(this);
            return;
        }

        Vector3 destinationPosition = nextHop.GetTransform().position;
        transform.position = Vector3.MoveTowards(transform.position, destinationPosition, speed * Time.deltaTime);

        // Check if the packet has reached its destination
        if (Vector3.Distance(transform.position, destinationPosition) < 0.1f)
        {
            // Deliver the packet
            nextHop.ReceivePacket(this);
            
        }
    }

    public void StartReturn()
    {
        isReturning = true;
        Debug.Log("Returning packet");
        // pastNodes.RemoveAt(pastNodes.Count - 1);
    }
}