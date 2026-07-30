// InfrastructureInstance.cs

using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using DefaultNamespace;
using DefaultNamespace.EnvGraphic;
using DefaultNamespace.Util;
using Infrastructure;
using MetaChallenges;
using Random = UnityEngine.Random;
using Stats;
using UI;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class InfrastructureInstance : WorldObjectBase, iAttackable
{
    public Color startColor;
   // public InfrastructureData data;

    protected SpriteRenderer spriteRenderer;

    public float CurrentLoad { get; set; }

    public InfraSize CurrentSize { get; private set; } = 0;

    public string Version = "0.0.1";
    public GameObject serverSmokeEffect;

    public Dictionary<NetworkPacketData.PType, List<NetworkConnection>> CurrConnections =
        new Dictionary<NetworkPacketData.PType, List<NetworkConnection>>();

    protected UIMetricsBubble metricsBubble;
    protected float costPerSecond = 0;
    protected float fractionalCost = 0;


    protected virtual void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            startColor = spriteRenderer.color;
        }
    }



    public virtual void FixedUpdate()
    {
        if (GameManager.Instance.UIManager.IsPausedState())
        {
            return;
        }
        if (CurrentState == WorldObjectBase.State.Operational)
        {
            CurrentLoad -= GetWorldObjectType().Stats.GetStatValue(StatType.Infra_LoadRecoveryRate) * GetSizeMultiplier() * Time.fixedDeltaTime;
            if (CurrentLoad < 0)
            {
                CurrentLoad = 0;
            }
        }

        if (GameManager.Instance.GameLoopManager.CurrentState != GameLoopManager.GameState.Play)
        {
            return;
        }
        if (!IsActive())
        {
            return;
        }
        // Check costs. 
        float tickCost = costPerSecond * Time.fixedDeltaTime;
        fractionalCost += tickCost;
        int fractionalCostFloor = (int)Math.Round(fractionalCost + tickCost);
        // Debug.Log($"tickCost: ${tickCost} -  fractionalCostFloor: {fractionalCostFloor}");
        if (fractionalCostFloor > 0)
        {
            // Spend fractionalCostFloor
            GameManager.Instance.IncrStat(StatType.Money, fractionalCostFloor * -1);
            GameManager.Instance.FloatingTextFactory.ShowText($"-${fractionalCostFloor} Cost", transform.position,
                Color.khaki);
            fractionalCost = fractionalCost - fractionalCostFloor;
        }
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

        if (CurrentState == State.Frozen)
        {
            packet.MarkFailedAndDestroy();
            packet.MoveToNextNode();
            TriggerSparks();
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
            GameManager.Instance.FloatingTextFactory.ShowText($"-${costPerPacket} Cost", transform.position,
                Color.khaki);
        }

        if (loadPerPacket != 0)
        {
            CurrentLoad += loadPerPacket;
            GameManager.Instance.FloatingTextFactory.ShowText($"+{loadPerPacket} Load", transform.position,
                spriteRenderer.color);

            if (CurrentLoad > GetMaxLoad())
            {
                packet.MarkFailedAndDestroy();
                CurrentLoad = GetMaxLoad();
                SetState(WorldObjectBase.State.Frozen);
                packet.MoveToNextNode();
                return false; // Stop processing
            }
        }
        
        return true; // Continue processing

    }

    private void TriggerSparks()
    {
        GameObject gameObject = GameManager.Instance.prefabManager.Create("Spark1Effect", transform.position);
        gameObject.transform.SetParent(transform);
        gameObject.transform.localPosition = new Vector3(0, 0, -1f);
    }

    protected virtual void RoutePacket(NetworkPacket packet)
    {
       
        WorldObjectType worldObjectType = GetWorldObjectType();
        if (worldObjectType.NetworkConnections != null && worldObjectType.NetworkConnections.Count > 0 &&
            CurrentState == State.Operational)
        {
            NetworkConnection connection = GetNextNetworkConnection(packet.data.Type);
            if (connection != null)
            {
                WorldObjectType.Type type = connection.worldObjectType;
         

                InfrastructureInstance nextReceiver = GameManager.Instance.GetRandomWorldObjectByType(type) as InfrastructureInstance;
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

    public virtual void Initialize()
    {
        


        Initialize(); // Ensure default stats are set up
        CurrentSize = InfraSize.Small;
        CurrentLoad = 0; // SetStatCollection current load
        UpdateAppearance();
        UpdateCostPerSecond();

    }

    public float UpdateCostPerSecond()
    {
       costPerSecond = GetDailyCost() / GameManager.Instance.GameLoopManager.GetDayDurationSeconds();

       return costPerSecond;
    }




    public override void SetState(WorldObjectBase.State newState)
    {
        if (CurrentState == newState) return; // No change
        State previousState = CurrentState;
        CurrentState = newState;
        if (serverSmokeEffect != null)
        {
            serverSmokeEffect.SetActive(false);
        }

        switch (newState)
        {
            case(State.Unlocked):
                attentionIconColor = Color.white;
                ShowAttentionIcon("Build");
                break;
            case (State.Operational):
                HideAttentionIcon();
                attentionIconColor = Color.white;
                CurrentLoad = 0;
                break;
            case (State.Planned):
                // Debug.Log($"!!!!{gameObject.name}: State Set To {newState}");
                break;
            case (State.Frozen):

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
                ShowAttentionIcon("Fix");
                GameManager.Instance.UIManager.TriggerScreenShake(1, .5f);
                // TODO Create a task automatically if you have researched CWAlarm
                break;
        }
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
       
        switch (CurrentState)
        {
            case State.Locked:
                // Ghosted / Outlined appearance
                spriteRenderer.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                break;
            case State.Unlocked:
                // Available to be planned
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.2f);
                break;
            case State.Planned:
                // Construction appearance
                spriteRenderer.color = new Color(1f, 0.8f, 0.3f, 0.5f);
                break;
            case State.Operational:
                // Normal appearance
                spriteRenderer.color = Color.white;
                break;
            case State.Frozen:
                spriteRenderer.color = Color.red;
                break;
        }

      
      
    }

    public override void OnWorldObjectStateChange(WorldObjectBase instance, State previousState)
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

            List<WorldObjectBase> instances = GameManager.Instance.GetWorldObjectByType(conn.worldObjectType);
            foreach (WorldObjectBase instance in instances)
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
        foreach (NetworkConnection conn in worldObjectType.NetworkConnections)
        {
            List<WorldObjectBase> instances = GameManager.Instance.GetWorldObjectByType(conn.worldObjectType);
            foreach (WorldObjectBase instance in instances)
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

        transform.localScale = GetLocalScale();

        
        if (GetWorldObjectType().GetMetaStat(MetaStat.Infra_MaxSize) < newSizeNumber)
        {
            GetWorldObjectType().SetMetaStat(MetaStat.Infra_MaxSize, newSizeNumber);
        }

       
        SetState(State.Operational);
        UpdateCostPerSecond();
        UpdateAppearance(); // Update visual state after resize
    }

    public virtual Vector3 GetLocalScale()
    {
        float visualScaleFactor = 1.0f + (1 - InfraSizeHelper.SizeToNumber(CurrentSize) * 0.3f);
        return Vector3.one * visualScaleFactor;
    }
    public override LevelUpEnvGraphic ShowLevelUpGraphic(Rarity rarity, UnityAction<Rarity, bool> _onDone = null)
    {
        LevelUpEnvGraphic levelUpEnvGraphic = base.ShowLevelUpGraphic(rarity, _onDone);
        levelUpEnvGraphic.transform.localScale = GetLocalScale();
        return levelUpEnvGraphic;
    }



    public override List<NPCTask> GetAvailableTasks()
    {

        List<NPCTask> availableTasks = new List<NPCTask>();
        switch (CurrentState)
        {
            case (State.Unlocked):
                availableTasks.Add(new BuildTask(this));
          
                break;
            case (State.Operational):
                AddResizeButtons(availableTasks);
                availableTasks.Add(new ShutdownTask(this));
                break;
            case (State.Frozen):
                availableTasks.Add(new FixFrozenTask(this));
                AddResizeButtons(availableTasks);
                break;
        }


        return availableTasks;
    }

    protected void AddResizeButtons(List<NPCTask> availableTasks)
    {
        WorldObjectType worldObjectType = GetWorldObjectType();
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

    }
    

    public void ReceiveAttack(NPCBase npcBase)
    {
        CurrentLoad += GetMaxLoad() / 4f;
        TriggerSparks();
        GameManager.Instance.UIManager.TriggerScreenShake();
    }

    public bool IsDead()
    {
        return CurrentState == State.Frozen;
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

        return Id;

    }



    public virtual UIMetricsBubble  ShowMetricsBubble()
    {
        if (metricsBubble != null)
        {
            metricsBubble.Close();
        }
        // gameObject.SetActive(true);
        if (GetWorldObjectType().networkPackets.Count == 0)
        {
            return null;
        }
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

    public override UIDialogBubble RenderDetailBubble()
    {
        UIDialogBubble dialogBubble = base.RenderDetailBubble();
       
        UIPanelLineProgressBar loadBar = dialogBubble.AddLine<UIPanelLineProgressBar>();
        loadBar.SetPreText($"CPU Load:");
        loadBar.OnGetProgress = () =>
        {
            return CurrentLoad/GetMaxLoad();
        };
        dialogBubble.AddLine<UIPanelLine>();

        List<NPCTask> tasks = GetAvailableTasks();
        if (tasks.Count > 0)
        {
            dialogBubble.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "Actions: ";
        }
        foreach (NPCTask task in tasks)
        {
            NPCTask localTask = task; 
            dialogBubble.AddButton(task.GetAssignButtonText(), () =>
            {
                GameManager.Instance.AddTask(localTask);
                HideAttentionIcon();
                HideDialogBubble();
            }).H3();
        }
        dialogBubble.AddLine<UIPanelLine>();
        /*dialogBubble.AddButton("Details", () =>
        {
            GameManager.Instance.UIManager.worldObjectDetailPanel.ShowWorldObjectDetail(this);
            HideAttentionIcon();
            HideDialogBubble();
        });*/
        UIPanelLine bottomLine = dialogBubble.AddLine<UIPanelLine>();
         UIPanelLineSectionButton detailButton = bottomLine.Add<UIPanelLineSectionButton>();
         detailButton.text.text = "Details";
         detailButton.button.onClick.AddListener(() =>
        {
            GameManager.Instance.UIManager.worldObjectDetailPanel.ShowWorldObjectDetail(this);
            HideAttentionIcon();
            HideDialogBubble();
        });
         
        UIPanelLineSectionButton closeButton = bottomLine.Add<UIPanelLineSectionButton>();
        closeButton.text.text = "Close";
        closeButton.button.onClick.AddListener(() =>
        {
            HideDialogBubble();
        });
  
        return dialogBubble;

    }

}