using UI;
using UnityEngine;

public class InternetPipe : InfrastructureInstance
{

    public enum InternetPipeState
    {
        Normal,
        DDoS
    }
    public InternetPipeState  State { get; protected set;  } = InternetPipeState.Normal;
    protected Animator animator;
    protected override void Awake()
    {
        base.Awake();
        if(animator == null) 
        {
            animator = GetComponentInChildren<Animator>();
        }
        
    }
    public override void Initialize(InfrastructureData infraData)
    {
        base.Initialize(infraData);
        MarkNormal();
    }

    public void MarkNormal()
    {
        State = InternetPipeState.Normal;
        animator.SetBool("isDDoS", false);
    }
    public void MarkDDoS()
    {
        State = InternetPipeState.DDoS;
        animator.SetBool("isDDoS", true);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (State == InternetPipeState.DDoS)
        {
            if (Random.value > 0.9f)
            {
                NetworkPacketData networkPacketData =
                    GameManager.Instance.GetNetworkPacketDataByType(NetworkPacketData.PType.MaliciousText);
                SendPacket(networkPacketData);
            }
        }
    }

    public NetworkPacket SendPacket(NetworkPacketData networkPacketData)
    {
        // int connectionCount = data.NetworkConnections?.Count ?? 0;
        NetworkConnection connection = GetNextNetworkConnection(networkPacketData.Type);
        if (connection == null)
        {
           Debug.LogError($"{gameObject.name} Could find  send packet {networkPacketData.Type} - CurrConnections: {CurrConnections.Count}");
            return null;
        }
                
   
        InfrastructureInstance targetReceiver = GameManager.Instance.GetRandomWorldObjectByType(connection.worldObjectType);
        if (targetReceiver == null)
        {
            Debug.LogError($"{gameObject.name} Could find world object {connection.worldObjectType}");
            return null;
        }
        
        // Create the packet
        string fileName = $"file_{networkPacketData.Type}_{Random.Range(1000, 9999)}.dat";
        int size = Random.Range(5, 50);
        NetworkPacket packet = GameManager.Instance.CreatePacket(networkPacketData, fileName, size, this);
                
        packet.SetNextTarget(targetReceiver);
        return packet;


    }

    // The InternetPipe itself doesn't "receive" packets in the traditional sense, it only generates them.
    // A packet arriving here has finished its journey. We override this method to destroy it.
    protected override bool HandleIncomingPacket(NetworkPacket packet)
    {
        // Destroy the packet as it has finished its journey.
        GameManager.Instance.DestroyPacket(packet);
        // Return false to prevent RoutePacket() and MoveToNextNode() from being called.
        return false;
    }
    
    public override void UpdateAppearance()
    {
        
       
        switch (data.CurrentState)
        {
            case InfrastructureData.State.Locked:
            case InfrastructureData.State.Unlocked:
                gameObject.SetActive(false);
                break;
            case InfrastructureData.State.Planned:
                gameObject.SetActive(true);
                spriteRenderer.color = new Color(1f, 0.8f, 0.3f, 0.5f);
                break;
            case InfrastructureData.State.Operational:
                gameObject.SetActive(true);
                spriteRenderer.color = Color.white;
                break;
        }

      
    }

    public override UIMetricsBubble ShowMetricsBubble()
    {
        return null;
    }

    public override Vector3 GetInteractionPosition(InteractionType interactionType =  InteractionType.Basic)
    {
        switch (interactionType)
        {
            case(InteractionType.Explain):
                return transform.position + new Vector3(3, 2, 0);
            default:
               return base.GetInteractionPosition(interactionType);
        }
        
    }

    public override void ShowAttentionIcon()
    {
    }
}
