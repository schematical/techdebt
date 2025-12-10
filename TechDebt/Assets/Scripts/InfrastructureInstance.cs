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
    
    public float CurrentLoad { get; set; }

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

    public void FixedUpdate()
    {
        if (data.CurrentState != InfrastructureData.State.Operational)
        {
            return;
        }
        CurrentLoad -= data.loadRecoveryRate * Time.fixedDeltaTime;
        if (CurrentLoad < 0)
        {
            CurrentLoad = 0;  
        }
        if (CurrentLoad > data.maxLoad)
        {
            CurrentLoad = data.maxLoad;  
        }
        // Debug.Log("CurrentLoad: " + CurrentLoad + " - " + data.loadRecoveryRate);
        float c = 1 - CurrentLoad / data.maxLoad;
        spriteRenderer.color = new Color(1, c, c, 1); 
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
        // Update load
        CurrentLoad += data.loadPerPacket;


        if (CurrentLoad > data.maxLoad)
        {
            packet.MarkFailed();
        }
        // If there are network connections, try to forward the packet
        if (data.NetworkConnections != null && data.NetworkConnections.Length > 0 && data.CurrentState == InfrastructureData.State.Operational)
        {
            string nextTargetId = GetNextNetworkTargetId();
            if (nextTargetId != null)
            {
                InfrastructureInstance nextReceiver = GameManager.Instance.GetInfrastructureInstanceByID(nextTargetId);
                if (nextReceiver != null && nextReceiver.data.CurrentState == InfrastructureData.State.Operational)
                {
                    packet.SetNextTarget(nextReceiver);
                }
                else
                {
                    Debug.LogWarning($"{data.DisplayName} cannot forward packet {packet.FileName}: Next receiver not found or not operational. Returning");
                    packet.StartReturn();
                }
            }
            else
            {
                Debug.LogWarning($"{data.DisplayName} cannot forward packet {packet.FileName}: No valid next target. Returning");
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

    public string GetNextNetworkTargetId()
    {
        Debug.Log("GetNextNetworkTargetId: " + data.NetworkConnections.Length);
        if (data.NetworkConnections == null || data.NetworkConnections.Length == 0)
        {
            return null;
        }

        int highestPriority = 0;
        foreach (var conn in data.NetworkConnections)
        {
            InfrastructureInstance instance = GameManager.Instance.GetInfrastructureInstanceByID(conn.TargetID);
            Debug.Log("GetNextNetworkTargetId 2: " + conn.TargetID + " - " +  conn.Priority + " - " + instance.data.CurrentState);
            if (
                instance != null &&
                instance.data.CurrentState == InfrastructureData.State.Operational &&
                conn.Priority > highestPriority)
            {
                highestPriority = conn.Priority;
            }
        }

        Debug.Log("Highest Prioity: " + highestPriority);
        var highPriorityConnections = new System.Collections.Generic.List<NetworkConnection>();
        foreach (var conn in data.NetworkConnections)
        {
            if (conn.Priority == highestPriority)
            {
                highPriorityConnections.Add(conn);
            }
        }

        if (highPriorityConnections.Count > 0)
        {
            return highPriorityConnections[Random.Range(0, highPriorityConnections.Count)].TargetID;
        }

        return null;
    }
}
