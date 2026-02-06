// NetworkPacket.cs
using UnityEngine;
using System.Collections.Generic;
using System.Data;
using UnityEngine.EventSystems;

public class NetworkPacket : MonoBehaviour, IPointerClickHandler, iTargetable
{
    public enum State { Running, Failed }
    public State CurrentState = State.Running;
    public string FileName { get; private set; }
    public NetworkPacketData data;
    public int Size { get; private set; } // In MB for simplicity
    public float Speed { get; private set; }
    public float BaseSpeed { get; private set; } = 2f;
    private SpriteRenderer spriteRenderer;
    public int returnIndex = -1;
    public Vector3 currentPosition; // Current position in world space
    public InfrastructureInstance nextHop; // The next destination for this packet
    
    public List<InfrastructureInstance> pastNodes = new List<InfrastructureInstance>();


	void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
       
    } 
    public virtual void Initialize(NetworkPacketData npData, string fileName, int size, InfrastructureInstance origin = null)
    {
        data = npData;
        FileName = fileName;
        Size = size;
        Speed = BaseSpeed;
        gameObject.name = $"Packet_{FileName}";
        pastNodes.Clear();
        pastNodes.Add(origin);
    }
	public bool IsReturning()
	{
		return returnIndex != -1;
	}
    public void MoveToNextNode()
    {
         
        if (returnIndex != -1)
        {
            
            nextHop = pastNodes[returnIndex];
            returnIndex -= 1;
        }
    }

    public void SetNextTarget(InfrastructureInstance target)
    {
        if (nextHop != null)
        {
            pastNodes.Add(nextHop);
        }

        nextHop = target;
  
    }
     protected virtual void Update()
    {
        if (GameManager.Instance.NetworkPacketState == GameManager.GlobalNetworkPacketState.Frozen)
        {
            return;
        }
        if (nextHop == null)
        {
            // If there's no destination, destroy the packet to prevent clutter
            CompleteTrip();
            return;
        }

        Vector3 destinationPosition = nextHop.GetInteractionPosition();
        transform.position = Vector3.MoveTowards(transform.position, destinationPosition, Speed * Time.deltaTime);
        float dist = Vector3.Distance(transform.position, destinationPosition);
        if (dist < 1f)
        {
            transform.localScale = new Vector3(dist,dist, 1);
        }
        else
        {
            if (transform.localScale.x < 1)
            {
                transform.localScale = new Vector3(transform.localScale.x + Time.deltaTime, transform.localScale.y + Time.deltaTime, 1);
            }
            else
            {
                transform.localScale = Vector3.one;
            }
        }
        // Check if the packet has reached its destination
        if (dist < 0.1f)
        {
            // Deliver the packet
            nextHop.ReceivePacket(this);
        }
    }

    private void CompleteTrip()
    {
        GameManager.Instance.DestroyPacket(this);
    }

    public void MarkFailed() {
		CurrentState = State.Failed;
		GameManager.Instance.DestroyPacket(this);
	}
    public virtual void StartReturn()
    {
        returnIndex = pastNodes.Count - 1;
    }
    

    public void Reset()
    {
        spriteRenderer.color = Color.white;
        returnIndex = -1;
        CurrentState = State.Running;
        nextHop = null;
        pastNodes.Clear();
    }

    public void SetSpeed(float speed)
    {
        Speed = speed;
        UpdateAppearance();
    }
    public void UpdateAppearance()
    {
        float loadPct = Speed /  BaseSpeed;
        spriteRenderer.color = new Color(1 - loadPct, 0,0, 0.2f);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        var cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null)
        {
            cameraController.StartFollowing(transform);
        }
    }

    public Vector3 GetInteractionPosition()
    {
        return transform.position;
    }
}