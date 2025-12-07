// InfrastructureInstance.cs
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class InfrastructureInstance : MonoBehaviour, IDataReceiver
{
    public InfrastructureData data;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        // Register with the routing manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterReceiver(data.ID, this);
        }
        else
        {
            Debug.LogError($"DIAGNOSTIC: InfrastructureInstance '{data.ID}' attempted to register, but GameManager.Instance was NULL.");
        }
    }

    void OnDestroy()
    {
        // Unregister when destroyed
        if (GameManager.Instance != null && GameManager.Instance.isQuitting == false)
        {
            GameManager.Instance.UnregisterReceiver(data.ID);
        }
    }

    public virtual void ReceivePacket(NetworkPacket packet)
    {
        Debug.Log($"{data.DisplayName} received packet: {packet.FileName}");

        // If there are network connections, try to forward the packet
        if (data.NetworkConnections != null && data.NetworkConnections.Length > 0 && data.CurrentState == InfrastructureData.State.Operational)
        {
            // For simplicity, let's just forward to the first connection for now
            string nextConnectionId = data.NetworkConnections[0];
            IDataReceiver nextReceiver = GameManager.Instance.GetReceiver(nextConnectionId);

            if (nextReceiver != null)
            {
                Debug.Log($"{data.DisplayName} forwarding packet {packet.FileName} to {nextConnectionId}");
                // Re-create the packet visual to move to the new destination
                GameManager.Instance.CreatePacket(packet.FileName, packet.Size, transform.position, nextReceiver);
                // The original packet's visual will be destroyed by its own script upon successful delivery.
            }
            else
            {
                Debug.LogWarning($"{data.DisplayName} cannot forward packet {packet.FileName}: Next receiver '{nextConnectionId}' not found.");
            }
        }
        else
        {
            Debug.Log($"{data.DisplayName} consumed packet {packet.FileName} (no connections or not operational).");
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void Initialize(InfrastructureData infraData)
    {
        this.data = infraData;
        UpdateAppearance();
    }

    public void SetState(InfrastructureData.State newState)
    {
        Debug.Log($"[SetState] for {gameObject.name}: Attempting to change state from {data.CurrentState} to {newState}.");
        data.CurrentState = newState;
        UpdateAppearance();
    }

    public void UpdateAppearance()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError($"[UpdateAppearance] for {gameObject.name}: spriteRenderer is NULL!");
            return;
        }

        Debug.Log($"[UpdateAppearance] for {gameObject.name}: Updating appearance for state {data.CurrentState}.");

        switch (data.CurrentState)
        {
            case InfrastructureData.State.Locked:
                // Ghosted / Outlined appearance
                spriteRenderer.color = new Color(0.5f, 0.5f, 1f, 0.3f); 
                break;
            case InfrastructureData.State.Unlocked:
                // Available to be planned
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.7f);
                break;
            case InfrastructureData.State.Planned:
                // Construction appearance
                spriteRenderer.color = new Color(1f, 0.8f, 0.3f, 0.7f); 
                break;
            case InfrastructureData.State.Operational:
                // Normal appearance
                spriteRenderer.color = Color.white; 
                break;
        }
        Debug.Log($"[UpdateAppearance] for {gameObject.name}: Final color is {spriteRenderer.color}.");
    }
}
