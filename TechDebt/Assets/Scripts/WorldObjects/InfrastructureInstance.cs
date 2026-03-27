// InfrastructureInstance.cs

using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using DefaultNamespace;
using DefaultNamespace.Util;
using Infrastructure;
using MetaChallenges;
using Random = UnityEngine.Random;
using Stats;
using UI;

public class InfrastructureInstance : WorldObjectBase, iAttackable
{
    public Color startcolor;
    public InfrastructureData data;

    protected SpriteRenderer spriteRenderer;

    public float CurrentLoad { get; set; }

    public InfraSize CurrentSize { get; private set; } = 0;

    public string Version = "0.0.1";
    public GameObject serverSmokeEffect;

    public Dictionary<NetworkPacketData.PType, List<NetworkConnection>> CurrConnections =
        new Dictionary<NetworkPacketData.PType, List<NetworkConnection>>();

    protected UIMetricsBubble metricsBubble;


    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            startcolor = spriteRenderer.color;
        }
    }



    public virtual void FixedUpdate()
    {
        if (GameManager.Instance.UIManager.IsPausedState())
        {
            return;
        }
        if (data.CurrentState == InfrastructureData.State.Operational)
        {
            CurrentLoad -= GetWorldObjectType().Stats.GetStatValue(StatType.Infra_LoadRecoveryRate) * Time.fixedDeltaTime;
            if (CurrentLoad < 0)
            {
                CurrentLoad = 0;
            }
        }


        /*if (IsActive())
        {
            // Debug.Log("CurrentLoad: " + CurrentLoad + " - " + data.loadRecoveryRate);
            float c = 1 - CurrentLoad / GetMaxLoad();
            spriteRenderer.color = new Color(1, c, c, 1);
        }*/
    }

    public float GetMaxLoad()
    {
        return GetWorldObjectType().Stats.GetStatValue(StatType.Infra_MaxLoad) * GetSizeMultiplier();
    }

    protected float GetSizeMultiplier()
    {   
        switch (CurrentSize)
        {
            case(InfraSize.Small):
                return 1;
            case(InfraSize.Medium):
                return 2;
            case(InfraSize.Large):
                return 4;
            default:
                throw new NotImplementedException();

        }
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
        if (packet.OnInfraContact(this) == NetworkPacket.NetworkPacketRouteAction.DefferToPacket)
        {
            return false;
        }

        if (data.CurrentState == InfrastructureData.State.Frozen)
        {
            packet.MarkFailedAndDestroy();
            packet.MoveToNextNode();
            GameObject gameObject = GameManager.Instance.prefabManager.Create("Spark1Effect", transform.position);
            gameObject.transform.SetParent(transform);
            gameObject.transform.localPosition = new Vector3(0, 0, -1f);
            return false; // Stop processing
        }

        if (packet.IsReturning())
        {
            return true; // Continue processing
        }

        WorldObjectType worldObjectType = GetWorldObjectType();
        float maxLoad = GetMaxLoad();
        if (CurrentLoad / maxLoad > worldObjectType.Stats.GetStatValue(StatType.Infra_LatencyStartsAtLoad))
        {
            
           float baseLine = worldObjectType.Stats.GetStatValue(StatType.Infra_LatencyStartsAtLoad) * maxLoad;
           float overLoad = CurrentLoad - baseLine;
           float penaltyPct = overLoad / (maxLoad - baseLine);
           float packetDelay = (packet.data.Stats.GetStatValue(StatType.NetworkPacket_LoadLatencyMultiplier) *
                                penaltyPct);
           packet.MarkDelayed(packetDelay);
        }
        worldObjectType.IncrMetaStat(MetaStat.Infra_HandleNetworkPacket);
        InfrastructureDataNetworkPacket packetData = worldObjectType.networkPackets.Find(p => p.PacketType == packet.data.Type);
        int loadPerPacket = 0;
        int costPerPacket = 0;
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

            if (CurrentLoad > GetMaxLoad())
            {
                packet.MarkFailedAndDestroy();
                CurrentLoad = GetMaxLoad();
                SetState(InfrastructureData.State.Frozen);
                packet.MoveToNextNode();
                return false; // Stop processing
            }
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

        Initialize(); // Ensure default stats are set up
        CurrentLoad = 0; // SetStatCollection current load
        UpdateAppearance();
      
    }




    public virtual void SetState(InfrastructureData.State newState)
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
                GameManager.Instance.UIManager.TriggerScreenShake(1, .5f);
                // TODO Create a task automatically if you have researched CWAlarm
                break;
        }
        // UpdateFootPrint();
        UpdateAppearance();
        GameManager.Instance.NotifyInfrastructureStateChange(this, previousState);
    }

    public virtual void UpdateAppearance()
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
            case InfrastructureData.State.Frozen:
                spriteRenderer.color = Color.red;
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
            string id = $"networkConnectionBonus_{foundConnection.networkPacketType}_{bonus.Stat}";// $"networkConnectionBonus_{bonus.Id}",
            StatModifier statModifier = worldObjectType.networkPackets[index].Stats.GetModifierByTypeAndId(bonus.Stat, id);
            if (statModifier == null)
            {
                worldObjectType.networkPackets[index].Stats.AddModifier(bonus.Stat, new StatModifier(
                    id, 
                    bonus.value, 
                    bonus.Type
                ));
            }
            
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
            int i = Random.Range(0, CurrConnections[pType].Count);
            NetworkConnection networkConnection = CurrConnections[pType][i];
            if (networkConnection == null)
            {
               // Debug.LogError($"Can't find network connection {pType} - i: {i}");
            }
            return networkConnection;
        }
        // Debug.LogError($"{gameObject.name} !ContainsKey: {pType}");
        return null;
    }

    public void ApplyResize(int sizeChange)
    {
        // Update and clamp the size level
        int newSizeNumber = InfraSizeHelper.SizeToNumber(CurrentSize) + sizeChange;
        CurrentSize = InfraSizeHelper.NumberToSize(newSizeNumber);
        float visualScaleFactor = 1.0f + (newSizeNumber * 0.3f);
        transform.localScale = Vector3.one * visualScaleFactor;

        
        if (GetWorldObjectType().GetMetaStat(MetaStat.Infra_MaxSize) < newSizeNumber)
        {
            GetWorldObjectType().SetMetaStat(MetaStat.Infra_MaxSize, newSizeNumber);
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
                InfraSize maxSize = worldObjectType.GetMaxSize();
                if (
                    CurrentSize != InfraSize.Small
                ) {
                    availableTasks.Add(new ResizeTask(this, -1));
                }

                if (
                    maxSize != InfraSize.Small &&
                    maxSize != CurrentSize 
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

    

    public void ReceiveAttack(NPCBase npcBase)
    {
        CurrentLoad += GetMaxLoad() / 4f;
    }

    public bool IsDead()
    {
        return data.CurrentState == InfrastructureData.State.Frozen;
    }
   

    public float GetDailyCost()
    {
        return GetWorldObjectType().Stats.GetStatValue(StatType.Infra_DailyCost) * GetSizeMultiplier();
    }
    public override string GetDisplayName()
    {
        WorldObjectType type = GetWorldObjectType();
        if (type.DisplayName != "Misc")
        {
            return type.DisplayName;
        }

        return data.Id;

    }
    public virtual UIMetricsBubble  ShowMetricsBubble()
    {
        if (metricsBubble != null)
        {
            metricsBubble.Close();
        }
        // gameObject.SetActive(true);

        metricsBubble = GameManager.Instance.prefabManager.Create("UIMetricsBubble", GetInteractionPosition(), GameManager.Instance.UIManager.transform).GetComponent<UIMetricsBubble>();
        metricsBubble.SetTarget(this);
        metricsBubble.transform.SetAsFirstSibling();
        metricsBubble.CleanUp();
        metricsBubble.Show();
        return metricsBubble;
    }

    public void HideMetricsBubble()
    {
        if (metricsBubble != null)
        {
            metricsBubble.Close();
        }
    }
    public bool IsMetricsBubbleActive()
    {
        if (metricsBubble == null)
        {
            return false;
        }
        return metricsBubble.gameObject.activeInHierarchy;
    }
    
}