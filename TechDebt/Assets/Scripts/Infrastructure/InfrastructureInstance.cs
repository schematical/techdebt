// InfrastructureInstance.cs

using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Infrastructure;
using MetaChallenges;using NPCs;
using NUnit.Framework;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;
using Stats;

public class InfrastructureInstance : WorldObjectBase
{
    public Color startcolor;
    public InfrastructureData data;

    private SpriteRenderer spriteRenderer;

    public float CurrentLoad { get; set; }

    public int CurrentSizeLevel { get; private set; } = 0;

    public string Version = "0.0.1";
    public GameObject serverSmokeEffect;

    public Dictionary<NetworkPacketData.PType, List<NetworkConnection>> CurrConnections =
        new Dictionary<NetworkPacketData.PType, List<NetworkConnection>>();
    
    public MetaStatCollection metaStatCollection = new MetaStatCollection();

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            startcolor = spriteRenderer.color;
        }
    }

    protected void Start()
    {
        foreach (var networkPacket in data.networkPackets)
        {
            networkPacket.Init();
        }

    }

    public void FixedUpdate()
    {
        if (data.CurrentState == InfrastructureData.State.Operational)
        {
            CurrentLoad -= data.Stats.GetStatValue(StatType.Infra_LoadRecoveryRate) * Time.fixedDeltaTime;
            if (CurrentLoad < 0)
            {
                CurrentLoad = 0;
            }
        }



        if (IsActive())
        {
            // Debug.Log("CurrentLoad: " + CurrentLoad + " - " + data.loadRecoveryRate);
            float c = 1 - CurrentLoad / data.Stats.GetStatValue(StatType.Infra_MaxLoad);
            spriteRenderer.color = new Color(1, c, c, 1);
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

  

  

    public void ReceivePacket(NetworkPacket packet)
    {
        if (!HandleIncomingPacket(packet))
        {
            // Packet was failed or instance is not in a state to process, so we stop here.
            // HandleIncomingPacket is responsible for calling MoveToNextNode in these cases.
            return;
        }

        RoutePacket(packet);

        packet.MoveToNextNode();
    }

    protected virtual bool HandleIncomingPacket(NetworkPacket packet)
    {
        if (data.CurrentState == InfrastructureData.State.Frozen)
        {
            packet.MarkFailed();
            packet.MoveToNextNode();
            GameObject gameObject = GameManager.Instance.prefabManager.Create("Spark1Effect", transform.position);
            gameObject.transform.SetParent(transform);
            gameObject.transform.localPosition = new Vector3(0, 0, -1f);
            return false; // Stop processing
        }

        if (!packet.IsReturning())
        {
            // --- Standard Load and Cost Calculation ---
            int loadPerPacket = data.LoadPerPacket;
            int costPerPacket = data.CostPerPacket;

            metaStatCollection.Incr(MetaStat.Infra_HandleNetworkPacket);
            var packetData = data.networkPackets.Find(p => p.PacketType == packet.data.Type);
            if (packetData != null)
            {
                loadPerPacket = (int)packetData.Stats.GetStatValue(StatType.Infra_LoadPerPacket);
                costPerPacket = (int)packetData.Stats.GetStatValue(StatType.Infra_PacketCost);
            }

            if (costPerPacket != 0)
            {
                GameManager.Instance.IncrStat(StatType.Money, costPerPacket * -1);
                GameManager.Instance.FloatingTextFactory.ShowText($"-${costPerPacket}", transform.position,
                    Color.khaki);
            } 

            if (loadPerPacket != 0)
            {
                CurrentLoad += loadPerPacket;
                GameManager.Instance.FloatingTextFactory.ShowText($"+{loadPerPacket}", transform.position,
                    spriteRenderer.color);

                if (CurrentLoad > data.MaxLoad)
                {
                    packet.MarkFailed();
                    CurrentLoad = data.Stats.GetStatValue(StatType.Infra_MaxLoad);
                    SetState(InfrastructureData.State.Frozen);
                    packet.MoveToNextNode();
                    return false; // Stop processing
                }
            }

            float loadPct = CurrentLoad / data.MaxLoad;
            if (loadPct > .5f)
            {
                packet.SetSpeed(packet.BaseSpeed * loadPct);
            }
        }

        return true; // Continue processing
    }

    protected virtual void RoutePacket(NetworkPacket packet)
    {
        if (data.NetworkConnections != null && data.NetworkConnections.Count > 0 &&
            data.CurrentState == InfrastructureData.State.Operational)
        {
            NetworkConnection connection = GetNextNetworkConnection(packet.data.Type);
            if (connection != null)
            {
                string nextTargetId = connection.TargetID;
                GameManager.Instance.IncrStat(StatType.Money, connection.Cost * -1);

                InfrastructureInstance nextReceiver = GameManager.Instance.GetInfrastructureInstanceByID(nextTargetId);
                if (nextReceiver != null && nextReceiver.IsActive())
                {
                    packet.SetNextTarget(nextReceiver);
                }
                else
                {

                    ReturnPacket(packet);
                }
            }
            else
            {
                ReturnPacket(packet);
            }
        }
        else
        {
            ReturnPacket(packet);
        }
    }

    protected virtual void ReturnPacket(NetworkPacket packet)
    {
        var packetData = data.networkPackets.Find(p => p.PacketType == packet.data.Type);
        if (packetData == null)
        {
            packet.StartReturn();
            return;
        }

        switch (packetData.RouteType)
        {
            case(InfrastructureDataNetworkPacket.NCRouteType.End):
                GameManager.Instance.DestroyPacket(packet);
                break;
            case(InfrastructureDataNetworkPacket.NCRouteType.Return):
            default:
                packet.StartReturn();
                break;
          
        }
    }

public Transform GetTransform()
    {
        return transform;
    }

    public virtual void Initialize(InfrastructureData infraData)
    {
        data = infraData;
        Initialize(); // Ensure default stats are set up
        CurrentLoad = 0; // Initialize current load
        UpdateAppearance();
    }
    public virtual void Initialize()
    {

        data.Stats.Add(new StatData(StatType.Infra_DailyCost, data.DailyCost));
        data.Stats.Add(new StatData(StatType.Infra_BuildTime, data.BuildTime));
        data.Stats.Add(new StatData(StatType.Infra_LoadPerPacket, data.LoadPerPacket));
        data.Stats.Add(new StatData(StatType.Infra_MaxLoad, data.MaxLoad));
        data.Stats.Add(new StatData(StatType.Infra_LoadRecoveryRate, data.LoadRecoveryRate));
        data.Stats.Add(new StatData(StatType.TechDebt, 0f));
        data.Stats.Add(new StatData(StatType.Infra_MaxSize, 2));// Todo get this number from a meta unlock.
    }


    public void SetState(InfrastructureData.State newState)
    {
        
        if (data.CurrentState == newState) return; // No change
        InfrastructureData.State previousState  = data.CurrentState;
        data.CurrentState = newState;
        if (newState == InfrastructureData.State.Operational)
        {
            // Create a new BuildTask and add it to the GameManager
         

            CurrentLoad = 0;
            if (serverSmokeEffect != null)
            {
                serverSmokeEffect.SetActive(false);
                serverSmokeEffect = null;
            }
        }
        if (newState == InfrastructureData.State.Planned)
        {
         
        }
        if (
            newState == InfrastructureData.State.Frozen
        )
        {
            
            GameObject explosionEffect = GameManager.Instance.prefabManager.Create("FireExplosion", transform.position);
            // be.transform.localPosition = Vector3.zero;
     
            explosionEffect.transform.SetParent(transform);
            explosionEffect.transform.localPosition = new Vector3(0, 0, -1f);
            
            serverSmokeEffect  = GameManager.Instance.prefabManager.Create("ServerSmokeEffect", transform.position);
            serverSmokeEffect.transform.SetParent(transform);
            serverSmokeEffect.transform.localPosition = new Vector3(0, 0, -1f);
          
            // TODO Create a task automatically if you have researched CWAlarm
            //var buildTask = new BuildTask(this, 7);
//GameManager.Instance.AddTask(buildTask);
        }

        UpdateAppearance();
        GameManager.Instance.NotifyInfrastructureStateChange(this, previousState);
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

    public void OnInfrastructureStateChange(InfrastructureInstance instance, InfrastructureData.State previousState)
    {
        if (
            !(
                IsActive()
            )
        )
        {
            return;
        }
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
       
            int index = data.networkPackets.FindIndex((packetData =>
            {
                if (packetData.PacketType == bonus.PacketType)
                {
                    return true;
                }

                return false;
            }));
            data.networkPackets[index].Stats.AddModifier(bonus.Stat, new StatModifier(bonus.Type, bonus.value));

        }
    }
    public void UpdateNetworkTargets()
    {
        // Debug.Log("GetNextNetworkTargetId: " + data.NetworkConnections.Length);
        if (data.NetworkConnections == null || data.NetworkConnections.Count() == 0)
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
                instance.IsActive() &&
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
                instance.IsActive()
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
    protected virtual NetworkConnection GetNextNetworkConnection(NetworkPacketData.PType pType) {
        if (CurrConnections.ContainsKey(pType))
        {
            return CurrConnections[pType][Random.Range(0, CurrConnections[pType].Count)];
        }

        return null;
    }

    public void ApplyResize(int sizeChange)
    {
        // Update and clamp the size level
        CurrentSizeLevel = Mathf.Clamp(CurrentSizeLevel + sizeChange, 0, (int) data.Stats.GetStatValue(StatType.Infra_MaxSize));

        // Calculate the visual scale factor (e.g., 1.25, 1.5, etc.)
        float visualScaleFactor = 1.0f + (CurrentSizeLevel * 0.25f);
        transform.localScale = Vector3.one * visualScaleFactor;

        // Remove existing resize modifiers to apply fresh ones
        data.Stats.RemoveModifiers(StatType.Infra_DailyCost, this);
        data.Stats.RemoveModifiers(StatType.Infra_MaxLoad, this);

        // Apply new modifiers only if the size is above base level
        if (CurrentSizeLevel > 0)
        {
            // Calculate the stat multiplier (doubles with each level: 2, 4, 8, 16)
            float statMultiplier = Mathf.Pow(2, CurrentSizeLevel);

            data.Stats.AddModifier(StatType.Infra_DailyCost, new StatModifier(StatModifier.ModifierType.Multiply, statMultiplier, this));
            data.Stats.AddModifier(StatType.Infra_MaxLoad, new StatModifier(StatModifier.ModifierType.Multiply, statMultiplier, this));
            data.Stats.AddModifier(StatType.Infra_LoadRecoveryRate, new StatModifier(StatModifier.ModifierType.Multiply, statMultiplier, this));
        }

        if (metaStatCollection.Get(MetaStat.Infra_MaxSize) < CurrentSizeLevel)
        {
            metaStatCollection.Set(MetaStat.Infra_MaxSize, CurrentSizeLevel);
        }
        SetState(InfrastructureData.State.Operational);
        UpdateAppearance(); // Update visual state after resize
        GameManager.Instance.NotifyDailyCostChanged(); // Recalculate and update daily cost display
    }

    public bool IsActive()
    {
        switch (data.CurrentState)
        {
            case(InfrastructureData.State.Operational):
            case(InfrastructureData.State.Frozen):
                return true;
            default:
                return false;
        }
    }

    public override List<NPCTask> GetAvailableTasks()
    {
        List<NPCTask> availableTasks = new List<NPCTask>();
        switch (data.CurrentState)
        {
            case(InfrastructureData.State.Unlocked): 
                // TODO: Add Build task
                availableTasks.Add(new BuildTask(this));
                break;
            case(InfrastructureData.State.Operational): 
                    // TODO: Add Resize tasks
                    if (CurrentSizeLevel > 0)
                    {
                        availableTasks.Add(new ResizeTask(this, -1));
                    }

                    if (CurrentSizeLevel < (int) data.Stats.GetStatValue(StatType.Infra_MaxSize))
                    {
                        availableTasks.Add(new ResizeTask(this, 1));
                    }

                    break;
            case(InfrastructureData.State.Frozen):
                    //TODO: Add a build task
                    availableTasks.Add(new FixFrozenTask(this));
                    break;
        }

        
        return availableTasks;

    }
}
