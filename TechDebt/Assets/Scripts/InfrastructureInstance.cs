// InfrastructureInstance.cs

using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class InfrastructureInstance : MonoBehaviour, IDataReceiver, /*IPointerEnterHandler, IPointerExitHandler, */IPointerClickHandler
{
    public Color startcolor;
    public InfrastructureData data;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            startcolor = spriteRenderer.color;
        }
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
            Debug.LogError($"InfrastructureInstance '{data.ID}' attempted to register, but GameManager.Instance was NULL.");
        }
    }

    /*
    public void OnPointerEnter(PointerEventData eventData)
    {
        startcolor = spriteRenderer.color;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.yellow;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = startcolor;
        }
    }
    */
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("IPointerClickHandler clicked: " + gameObject.name + " " + data.ID + " " + data.CurrentState);
        
        // This is where the tooltip logic should be handled.
        // We find the UIManager and tell it to show the tooltip for this specific instance.
        UIManager uiManager = GameManager.Instance.UIManager;
        if (uiManager != null)
        {
            uiManager.ShowInfrastructureTooltip(this);
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
        // If there are network connections, try to forward the packet
        if (data.NetworkConnections != null && data.NetworkConnections.Length > 0 && data.CurrentState == InfrastructureData.State.Operational)
        {
            // For simplicity, let's just forward to the first connection for now
            List<InfrastructureInstance> connections = new List<InfrastructureInstance>();
            foreach (var connectionId in data.NetworkConnections)
            {
                InfrastructureInstance connection = GameManager.Instance.GetInfrastructureInstanceByID(connectionId);
                if (connection != null && connection.data.CurrentState == InfrastructureData.State.Operational) connections.Add(connection);
            }
            if(connections.Count > 0) {
                int i = Random.Range(0, connections.Count);
                InfrastructureInstance nextReceiver = connections[i];

                // Re-create the packet visual to move to the new destination
                packet.SetNextTarget(nextReceiver);
                // The original packet's visual will be destroyed by its own script upon successful delivery.
            }
            else
            {
                Debug.LogWarning($"{data.DisplayName} cannot forward packet {packet.FileName}: Next receiver not found. Returning");
                packet.StartReturn();
            }
        }
        else
        {
            packet.StartReturn(); 
        }

        packet.MoveToNextNode();
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void Initialize(InfrastructureData infraData)
    {
        this.data = infraData;
        Debug.Log($"2222 Initialized infrastructure {data.ID} {data.CurrentState}");
        UpdateAppearance();
    }

    public void SetState(InfrastructureData.State newState)
    {
        if (data.CurrentState == newState) return; // No change

        data.CurrentState = newState;

        if (newState == InfrastructureData.State.Planned)
        {
            // Create a new BuildTask and add it to the GameManager
            var buildTask = new BuildTask(this);
            GameManager.Instance.AddTask(buildTask);
        }

        UpdateAppearance();
    }

    public void UpdateAppearance()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError($"[UpdateAppearance] for {gameObject.name}: spriteRenderer is NULL!");
            return;
        }

        switch (data.CurrentState)
        {
            case InfrastructureData.State.Locked:
                // Ghosted / Outlined appearance
                spriteRenderer.color = new Color(0.3f, 0.3f, 0.3f, 0.5f); 
                break;
            case InfrastructureData.State.Unlocked:
                // Available to be planned
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.2f);
                break;
            case InfrastructureData.State.Planned:
                // Construction appearance
                spriteRenderer.color = new Color(1f, 0.8f, 0.3f, 0.5f); 
                break;
            case InfrastructureData.State.Operational:
                // Normal appearance
                spriteRenderer.color = Color.white; 
                break;
        }
    }
}
