// InfrastructureInstance.cs

using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Infrastructure;
using MetaChallenges;
using Random = UnityEngine.Random;
using Stats;

public class InfrastructureInstance : WorldObjectBase, iAttackable
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



    public void FixedUpdate()
    {
        if (data.CurrentState == InfrastructureData.State.Operational)
        {
            CurrentLoad -= GetWorldObjectType().Stats.GetStatValue(StatType.Infra_LoadRecoveryRate) * Time.fixedDeltaTime;
            if (CurrentLoad < 0)
            {
                CurrentLoad = 0;
            }
        }


        if (IsActive())
        {
            // Debug.Log("CurrentLoad: " + CurrentLoad + " - " + data.loadRecoveryRate);
            float c = 1 - CurrentLoad / GetMaxLoad();
            spriteRenderer.color = new Color(1, c, c, 1);
        }
    }

    public float GetMaxLoad()
    {
        return GetWorldObjectType().Stats.GetStatValue(StatType.Infra_MaxLoad) * GetSizeMultiplier();
    }

    protected float GetSizeMultiplier()
    {
        return (float) Math.Pow(2, CurrentSizeLevel);
    }

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
            packet.MarkFailedAndDestroy();
            packet.MoveToNextNode();
            GameObject gameObject = GameManager.Instance.prefabManager.Create("Spark1Effect", transform.position);
            gameObject.transform.SetParent(transform);
            gameObject.transform.localPosition = new Vector3(0, 0, -1f);
            return false; // Stop processing
        }

        if (!packet.IsReturning())
        {
           
            metaStatCollection.Incr(MetaStat.Infra_HandleNetworkPacket);
            InfrastructureDataNetworkPacket packetData = GetWorldObjectType().networkPackets.Find(p => p.PacketType == packet.data.Type);
            int loadPerPacket = 0;
            int costPerPacket = 0;
            if (packetData != null)
            {
                 loadPerPacket = (int)packetData.Stats.GetStatValue(StatType.Infra_LoadPerPacket);
                 costPerPacket = (int)packetData.Stats.GetStatValue(StatType.Infra_PacketCost);
            }

            if (packet.data.Type == NetworkPacketData.PType.Purchase)
            {
                Debug.Log($"Load per packet: {loadPerPacket}");
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

                if (CurrentLoad > GetMaxLoad())
                {
                    packet.MarkFailedAndDestroy();
                    CurrentLoad = GetMaxLoad();
                    SetState(InfrastructureData.State.Frozen);
                    packet.MoveToNextNode();
                    return false; // Stop processing
                }
            }

            /*float loadPct = CurrentLoad / GetWorldObjectType().Stats.GetStatValue(StatType.Infra_MaxLoad);
            if (loadPct > .5f)
            {
                packet.SetSpeed(packet.BaseSpeed * loadPct);
            }*/
        }

        return true; // Continue processing
    }

    protected virtual void RoutePacket(NetworkPacket packet)
    {
        WorldObjectType worldObjectType = GetWorldObjectType();
        if (worldObjectType.NetworkConnections != null && worldObjectType.NetworkConnections.Count > 0 &&
            data.CurrentState == InfrastructureData.State.Operational)
        {
            NetworkConnection connection = GetNextNetworkConnection(packet.data.Type);
            if (connection != null)
            {
                WorldObjectType.Type type = connection.worldObjectType;
                // GameManager.Instance.IncrStat(StatType.Money, connection.cost * -1); // TODO: Work this back in

                InfrastructureInstance nextReceiver = GameManager.Instance.GetRandomWorldObjectByType(type);
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
        InfrastructureDataNetworkPacket packetData = GetWorldObjectType().networkPackets.Find(p => p.PacketType == packet.data.Type);
        if (packetData == null)
        {
            packet.StartReturn();
            return;
        }

        switch (packetData.RouteType)
        {
            case (InfrastructureDataNetworkPacket.NCRouteType.End):
                GameManager.Instance.DestroyPacket(packet);
                break;
            case (InfrastructureDataNetworkPacket.NCRouteType.Return):
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
        Type = data.worldObjectType;
        WorldObjectType worldObjectType = GetWorldObjectType();
        interactionPositionOffset = worldObjectType.interactionPositionOffset;
        Initialize(); // Ensure default stats are set up
        CurrentLoad = 0; // Initialize current load
        UpdateAppearance();
      
    }

    /*
    public override void Initialize()
    {
        WorldObjectType worldObjectType = GetWorldObjectType();
        GetWorldObjectType().Stats.Add(new StatData(StatType.Infra_DailyCost, data.DailyCost));
        GetWorldObjectType().Stats.Add(new StatData(StatType.Infra_BuildTime, data.BuildTime));
        GetWorldObjectType().Stats.Add(new StatData(StatType.Infra_LoadPerPacket, data.LoadPerPacket));
        GetWorldObjectType().Stats.Add(new StatData(StatType.Infra_MaxLoad, data.MaxLoad));
        GetWorldObjectType().Stats.Add(new StatData(StatType.Infra_LoadRecoveryRate, data.LoadRecoveryRate));
        GetWorldObjectType().Stats.Add(new StatData(StatType.TechDebt, 0f));
        GetWorldObjectType().Stats.Add(new StatData(StatType.Infra_MaxSize, 2)); // Todo get this number from a meta unlock.
        base.Initialize();
    }
    */


    public void SetState(InfrastructureData.State newState)
    {
        if (data.CurrentState == newState) return; // No change
        InfrastructureData.State previousState = data.CurrentState;
        data.CurrentState = newState;
        if (serverSmokeEffect != null)
        {
            serverSmokeEffect.SetActive(false);
        }

        switch (newState)
        {
            case(InfrastructureData.State.Unlocked):
                attentionIconColor = Color.white;
                ShowAttentionIcon();
                break;
            case (InfrastructureData.State.Operational):
                HideAttentionIcon();
                attentionIconColor = Color.white;
                CurrentLoad = 0;
                break;
            case (InfrastructureData.State.Planned):
                break;
            case (InfrastructureData.State.Frozen):

                GameObject explosionEffect =
                    GameManager.Instance.prefabManager.Create("FireExplosion", transform.position);

                explosionEffect.transform.SetParent(transform);
                explosionEffect.transform.localPosition = new Vector3(0, 0, -1f);
                if (
                    serverSmokeEffect == null ||
                    serverSmokeEffect.activeSelf
                )
                {
                    serverSmokeEffect =
                        GameManager.Instance.prefabManager.Create("ServerSmokeEffect", transform.position);
                    serverSmokeEffect.transform.SetParent(transform);
                    serverSmokeEffect.transform.localPosition = new Vector3(0, 0, -1f);
                }
                else
                {
                    serverSmokeEffect.gameObject.SetActive(true);
                }
                attentionIconColor = Color.red;
                ShowAttentionIcon();

                // TODO Create a task automatically if you have researched CWAlarm
                break;
        }
        // UpdateFootPrint();
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
        foreach (List<NetworkConnection> conn in CurrConnections.Values)
        {
            foundConnection = conn.Find((connection =>
            {
                if (connection.worldObjectType == instance.Type)
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
        WorldObjectType worldObjectType = GetWorldObjectType();
        foreach (NetworkConnectionBonus bonus in foundConnection.networkConnectionBonus)
        {
            int index = worldObjectType.networkPackets.FindIndex((packetData =>
            {
                if (packetData.PacketType == foundConnection.networkPacketType)
                {
                    return true;
                }

                return false;
            }));
            worldObjectType.networkPackets[index].Stats.AddModifier(bonus.Stat, new StatModifier(
                $"networkConnectionBonus_{foundConnection.networkPacketType}_{bonus.Stat}", // $"networkConnectionBonus_{bonus.Id}",
                bonus.value, 
                bonus.Type
            ));
        }
    }

    public void UpdateNetworkTargets()
    {
        WorldObjectType worldObjectType = GetWorldObjectType();
        // Debug.Log("GetNextNetworkTargetId: " + data.NetworkConnections.Length);
        if (worldObjectType.NetworkConnections == null || worldObjectType.NetworkConnections.Count() == 0)
        {
            return;
        }

        Dictionary<NetworkPacketData.PType, int> priorities = new Dictionary<NetworkPacketData.PType, int>();

        foreach (NetworkConnection conn in worldObjectType.NetworkConnections)
        {
            if (!priorities.ContainsKey(conn.networkPacketType))
            {
                priorities.Add(conn.networkPacketType, 0);
            }

            List<InfrastructureInstance> instances = GameManager.Instance.GetWorldObjectByType(conn.worldObjectType);
            foreach (InfrastructureInstance instance in instances)
            {
                if (
                    instance != null &&
                    instance.IsActive() &&
                    conn.priority > priorities[conn.networkPacketType]
                )
                {
                    priorities[conn.networkPacketType] = conn.priority;
                }
            }
           
        }

        CurrConnections = new Dictionary<NetworkPacketData.PType, List<NetworkConnection>>();
        foreach (var conn in worldObjectType.NetworkConnections)
        {
            List<InfrastructureInstance> instances = GameManager.Instance.GetWorldObjectByType(conn.worldObjectType);
            foreach (InfrastructureInstance instance in instances)
            {
                if (
                    conn.priority == priorities[conn.networkPacketType] &&
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
    }

    protected virtual NetworkConnection GetNextNetworkConnection(NetworkPacketData.PType pType)
    {
        if (CurrConnections.ContainsKey(pType))
        {
            return CurrConnections[pType][Random.Range(0, CurrConnections[pType].Count)];
        }

        return null;
    }

    public void ApplyResize(int sizeChange)
    {
        // Update and clamp the size level
        CurrentSizeLevel = Mathf.Clamp(CurrentSizeLevel + sizeChange, 0,
            (int)GetWorldObjectType().Stats.GetStatValue(StatType.Infra_MaxSize));

        // Calculate the visual scale factor (e.g., 1.25, 1.5, etc.)
        float visualScaleFactor = 1.0f + (CurrentSizeLevel * 0.25f);
        transform.localScale = Vector3.one * visualScaleFactor;

        
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
            case (InfrastructureData.State.Operational):
            case (InfrastructureData.State.Frozen):
                return true;
            default:
                return false;
        }
    }

    public override List<NPCTask> GetAvailableTasks()
    {
        WorldObjectType worldObjectType = GetWorldObjectType();
        List<NPCTask> availableTasks = new List<NPCTask>();
        switch (data.CurrentState)
        {
            case (InfrastructureData.State.Unlocked):
                availableTasks.Add(new BuildTask(this));
          
                break;
            case (InfrastructureData.State.Operational):
                if (
                    worldObjectType.CanBeUpsized && 
                    CurrentSizeLevel > 0
                ) {
                    availableTasks.Add(new ResizeTask(this, -1));
                }

                if (
                    worldObjectType.CanBeUpsized && 
                    CurrentSizeLevel < (int)GetWorldObjectType().Stats.GetStatValue(StatType.Infra_MaxSize)
                )
                {
                    availableTasks.Add(new ResizeTask(this, 1));
                }

                break;
            case (InfrastructureData.State.Frozen):
                availableTasks.Add(new FixFrozenTask(this));
                
                break;
        }


        return availableTasks;
    }

    public override string GetDetailText()
    {
        WorldObjectType type = GetWorldObjectType();
        string content = $"<b>{type.DisplayName}</b>\n";
        content += $"State: {data.CurrentState}\n\n";
        content += $"Release: {Version}\n\n";
        content += $"Curr Load: {CurrentLoad}/{GetMaxLoad()}\n";
        content += $"Daily Cost: ${GetDailyCost()}\n";
        content += $"Curr Size: {CurrentSizeLevel}\n";
        content += "<b>Stats:</b>\n";
        foreach (StatData stat in type.Stats.Stats.Values)
        {
            content += $"- {stat.Type}: {stat.Value:F2} (Base: {stat.BaseValue:F2})\n";
            if (stat.Modifiers.Count > 0)
            {
                content += "  <i>Modifiers:</i>\n";
                foreach (StatModifier modifier in stat.Modifiers)
                {
                    content += $"  - {modifier.Id} -{modifier.Value:F2} ({modifier.Type})\n";
                }
            }
        }
        content += "<b>NetworkPackets:</b>\n";
        foreach (InfrastructureDataNetworkPacket packet in type.networkPackets)
        {
            content += $"- {packet.PacketType}\n";//  Load Per Packet: {packet.loadPerPacket} - {packet.RouteType}\n";
            {
                content += "  <i>- Stats:</i>\n";
                foreach (StatData stat in packet.Stats.Stats.Values)
                {
                    content += $"  - {stat.Type}  - Base: {stat.BaseValue} - Calculated: {stat.Value}\n";
                    if (stat.Modifiers.Count > 0)
                    {
                        content += "  <i>   - Modifiers:</i>\n";
                        foreach (StatModifier modifier in stat.Modifiers)
                        {
                            content += $"      - {modifier.Id} - {modifier.GetDisplayText()}\n";
                        }
                    }
                }
            }
        }
      

        content += "\n<b>Connections:</b>\n";
        if (CurrConnections.Count == 0)
        {
            content += "No active connections.";
        }
        else
        {
            foreach (KeyValuePair<NetworkPacketData.PType, List<NetworkConnection>> kvp in CurrConnections)
            {
                List<NetworkConnection> networkConnections = kvp.Value;
                content += $"- <b>{kvp.Key}:</b>\n";
                foreach (NetworkConnection networkConnection in networkConnections)
                {
                    content += $"-- {networkConnection.networkPacketType} - {networkConnection.worldObjectType} \n";
                    foreach (NetworkConnectionBonus networkConnectionBonus in networkConnection.networkConnectionBonus)
                    {
                        content +=
                            $"--- {networkConnectionBonus.Stat} | {networkConnectionBonus.value}\\n";
                    }
                }

                content += "\n";
            }
        }


        return content;
    }


    public void ReceiveAttack(NPCBase npcBase)
    {
        CurrentLoad += GetMaxLoad() / 4f;
    }

    public bool IsDead()
    {
        return data.CurrentState == InfrastructureData.State.Frozen;
    }
    public override void UpdateFootPrint()
    {
            
        foreach (Vector3Int pos in GetFootPrint())
        {
            GameManager.Instance.gridManager.UpdateTileState(Vector3Int.FloorToInt(GridPosition + pos), !IsActive());
        }
    }

    public float GetDailyCost()
    {
        return GetWorldObjectType().Stats.GetStatValue(StatType.Infra_DailyCost) * GetSizeMultiplier();
    }
}