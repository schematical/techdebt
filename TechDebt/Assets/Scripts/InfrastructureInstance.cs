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


    public Dictionary<NetworkPacketData.PType, List<NetworkConnection>> CurrConnections = new Dictionary<NetworkPacketData.PType, List<NetworkConnection>>();

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
        if (data.CurrentState == InfrastructureData.State.Frozen)
        {
            packet.MarkFailed();
            packet.MoveToNextNode();
            return;
        }
        // Update load
        if (!packet.IsReturning())
        {
            float loadPerPacket = data.loadPerPacket;
            if (
                data.networkPackets != null &&
                data.networkPackets.Count() > 0
            )
            {
                InfrastructureDataNetworkPacket packetData = data.networkPackets.Find((packetData => {
                    if(packetData.PacketType == packet.data.Type)
                    {
                        return true;
                    }

                    return false;
                }));
                if (packetData != null)
                {
                    loadPerPacket = packetData.loadPerPacket;
                }
            }

            CurrentLoad += loadPerPacket;
            if (CurrentLoad > data.maxLoad)
            {
                packet.MarkFailed();
                SetState(InfrastructureData.State.Frozen);
            }
        }


       
        // If there are network connections, try to forward the packet
        if (data.NetworkConnections != null && data.NetworkConnections.Length > 0 && data.CurrentState == InfrastructureData.State.Operational)
        {
            string nextTargetId = GetNextNetworkTargetId(packet.data.Type);
            if (nextTargetId != null)
            {
             
                InfrastructureInstance nextReceiver = GameManager.Instance.GetInfrastructureInstanceByID(nextTargetId);
                if (nextReceiver != null && nextReceiver.data.CurrentState == InfrastructureData.State.Operational)
                {
                    
                    packet.SetNextTarget(nextReceiver);
                }
                else
                {
                    Debug.LogWarning($"{data.DisplayName} cannot forward packet {packet.FileName}: Next receiver not found or not operational. Returning - " + nextTargetId + "  --> " + ((nextReceiver != null) ? nextReceiver.data.CurrentState : ""));
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
            Debug.Log(
                $"{data.DisplayName} starting return - NetConnections: ${data.NetworkConnections.Length}");
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
        data = infraData;

        UpdateAppearance();
    }

    public void SetState(InfrastructureData.State newState)
    {
        if (data.CurrentState == newState) return; // No change

        data.CurrentState = newState;
        if (newState == InfrastructureData.State.Operational)
        {
            // Create a new BuildTask and add it to the GameManager
            CurrentLoad = 0;
        }
        if (newState == InfrastructureData.State.Planned)
        {
            // Create a new BuildTask and add it to the GameManager
            var buildTask = new BuildTask(this);
            GameManager.Instance.AddTask(buildTask);
        }
        if (newState == InfrastructureData.State.Frozen)
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

    public void OnInfrastructureBuilt(InfrastructureInstance instance)
    {
        UpdateNetworkTargets();
        

        // Filter CurrConnections to see if instance is in the list

        NetworkConnection foundConnection = null;
        foreach(var conn in CurrConnections.Values) {
            foundConnection = conn.Find((connection => {
                if (connection.TargetID == instance.data.ID)
                {
                    return true;
                }

                return false;
            }));
            if (foundConnection != null)
            {
                break;
            }
        }
        if (foundConnection == null)
        {
            return;
        }

        foreach (var bonus in foundConnection.NetworkConnectionBonus)
        {
            switch (bonus.Type)
            {
                case(NetworkConnectionBonus.BonusType.Multiplier):

                    switch (bonus.Stat)
                    {
                        case(NetworkConnectionBonus.InfrStat.LoadPerPacket):
                            if (bonus.PacketType == null)
                            {
                                data.loadPerPacket *= bonus.value;
                            }
                            else
                            {
                                InfrastructureDataNetworkPacket packetData = data.networkPackets.Find((packetData => {
                                    if(packetData.PacketType == bonus.PacketType)
                                    {
                                        return true;
                                    }

                                    return false;
                                }));
                                packetData.loadPerPacket = (int) Math.Round(bonus.value * packetData.loadPerPacket);
                            }

                            Debug.Log("Applied Bonus: " + data.loadPerPacket + " - " + bonus.PacketType);
                            break;
                        default:
                            throw new SystemException("TOOD: Write me");
                    }
                    break;
                default:
                    throw new SystemException("TOOD: Write me");
            }
        }
    }
    public void UpdateNetworkTargets()
    {
        // Debug.Log("GetNextNetworkTargetId: " + data.NetworkConnections.Length);
        if (data.NetworkConnections == null || data.NetworkConnections.Length == 0)
        {
            return;
        }
        Dictionary<NetworkPacketData.PType, int> priorities = new Dictionary<NetworkPacketData.PType, int>();
     
        foreach (var conn in data.NetworkConnections)
        {
            if (!priorities.ContainsKey(conn.networkPacketType))
            {
                priorities.Add(conn.networkPacketType, 0);
            }
            InfrastructureInstance instance = GameManager.Instance.GetInfrastructureInstanceByID(conn.TargetID);
        
            if (
                instance != null &&
                instance.data.CurrentState == InfrastructureData.State.Operational &&
                conn.Priority > priorities[conn.networkPacketType]
            )
            {
                priorities[conn.networkPacketType] = conn.Priority;
            }
        }

        CurrConnections = new Dictionary<NetworkPacketData.PType, List<NetworkConnection>>();
        foreach (var conn in data.NetworkConnections)
        {
            InfrastructureInstance instance = GameManager.Instance.GetInfrastructureInstanceByID(conn.TargetID);
            if (
                conn.Priority == priorities[conn.networkPacketType] &&
                instance != null &&
                instance.data.CurrentState == InfrastructureData.State.Operational
            )
            {
                if (!CurrConnections.ContainsKey(conn.networkPacketType))
                {
                    CurrConnections.Add(conn.networkPacketType, new List<NetworkConnection>());
                }
                CurrConnections[conn.networkPacketType].Add(conn);
            }
        }
    }
    public string GetNextNetworkTargetId(NetworkPacketData.PType pType) {
        if (CurrConnections.ContainsKey(pType))
        {
            return CurrConnections[pType][Random.Range(0, CurrConnections[pType].Count)].TargetID;
        }

        return null;
    }
}
