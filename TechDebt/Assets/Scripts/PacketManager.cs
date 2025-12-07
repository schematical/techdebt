// PacketManager.cs
using UnityEngine;
using System.Collections.Generic;

public class PacketManager : MonoBehaviour
{
    public static PacketManager Instance { get; private set; }

    public GameObject packetPrefab; // Assign a prefab in the inspector
    private List<NetworkPacket> activePackets = new List<NetworkPacket>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        // Create a default prefab if one isn't assigned
        if (packetPrefab == null)
        {
            packetPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            packetPrefab.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            packetPrefab.GetComponent<Renderer>().material.color = Color.cyan;
            // Add the NetworkPacket component to the dynamically created prefab
            packetPrefab.AddComponent<NetworkPacket>();
            packetPrefab.SetActive(false); // Use it as a template, not an active object
        }
        else
        {
            // Ensure the assigned prefab has the NetworkPacket component
            if (packetPrefab.GetComponent<NetworkPacket>() == null)
            {
                packetPrefab.AddComponent<NetworkPacket>();
            }
        }
    }

    // Method to create a new packet from scratch
    public void CreatePacket(string fileName, int size, Vector3 startPosition, IDataReceiver destination)
    {
        GameObject packetGO = Instantiate(packetPrefab, startPosition, Quaternion.identity);
        packetGO.SetActive(true);
        NetworkPacket packet = packetGO.GetComponent<NetworkPacket>();
        if (packet == null)
        {
            packet = packetGO.AddComponent<NetworkPacket>();
        }
        
        packet.Initialize(fileName, size, startPosition, destination);
        activePackets.Add(packet);
    }

    public void DestroyPacket(NetworkPacket packet)
    {
        activePackets.Remove(packet);
        Destroy(packet.gameObject);
    }
}