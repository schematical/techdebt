// DeliverItemTask.cs
using UnityEngine;
using System.Linq;

public class DeliverItemTask : NPCTask
{
    private enum DeliveryState
    {
        MovingToDropOff,
        PlacingItem,
        ReturningToDoor
    }

    private DeliveryState _currentState;
    private readonly InfrastructureInstance _doorInstance;
    private Vector3 _dropOffPosition;

    public DeliverItemTask() : base(null) // Destination is dynamic, so start with null
    {
        _doorInstance = GameManager.Instance.GetInfrastructureInstanceByID("door");
        if (_doorInstance == null)
        {
            Debug.LogError("DeliverItemTask cannot function without a 'door' infrastructure.");
            CurrentState = State.Completed; // End the task if there's no door
            return;
        }

        Priority = 10; 
        _currentState = DeliveryState.MovingToDropOff;
    }
    
    public override void OnStart(NPCBase npc)
    {
        if (_doorInstance == null) return;
        
        // Find a random walkable position within 20 units of the door.
        _dropOffPosition = GetRandomWalkablePoint(_doorInstance.transform.position, 10f);
        if (_dropOffPosition == Vector3.zero)
        {
            Debug.LogWarning("Could not find a valid drop-off point for delivery. Aborting.");
            CurrentState = State.Completed;
            return;
        }

    
        base.OnStart(npc); // Start moving to the drop-off position
        npc.MoveTo(_dropOffPosition);
    }

    public override void OnUpdate(NPCBase npc)
    {
        if (!isCloseEnough()) return;

        switch (_currentState)
        {
            case DeliveryState.MovingToDropOff:
                // Arrived at the drop-off point, now place the item.
                _currentState = DeliveryState.PlacingItem;
                GameManager.Instance.prefabManager.Create("BoxItem", _dropOffPosition);
                
                // Now, set the destination back to the door and start moving.
                _currentState = DeliveryState.ReturningToDoor;
                this.target = _doorInstance;
                base.OnStart(npc); 
                break;
            
            case DeliveryState.ReturningToDoor:
                // This state is handled by IsFinished when the NPC arrives at the door.
                break;
        }
    }
    public bool isCloseEnough()
    {
       

        switch (_currentState)
        {
            case DeliveryState.MovingToDropOff:
                return Vector3.Distance(_dropOffPosition, AssignedNPC.transform.position) < maxTaskRange;
                break;
            
            case DeliveryState.ReturningToDoor:
                return base.IsCloseEnough();
                break;
            default:
                throw new System.NotImplementedException();
        }
        
    }

    public override bool IsFinished(NPCBase npc)
    {
        // The task is finished when the NPC has returned to the door.
        return _currentState == DeliveryState.ReturningToDoor && isCloseEnough();
    }

    public override void OnEnd(NPCBase npc)
    {
        base.OnEnd(npc);
        // Deactivate the NPC once it has finished its delivery.
        npc.gameObject.SetActive(false); 
    }

    private Vector3 GetRandomWalkablePoint(Vector3 origin, float radius)
    {
        for (int i = 0; i < 50; i++) // 50 attempts to find a spot
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += origin;
            
            // Ensure the position is not inside an existing infrastructure's bounds
            bool isOccupied = GameManager.Instance.ActiveInfrastructure
                .Any(infra => infra.GetComponent<Collider2D>().bounds.Contains(randomDirection));

            if (!isOccupied)
            {
                 Node node = GridManager.Instance.NodeFromWorldPoint(randomDirection);
                 if (node != null && node.isWalkable)
                 {
                     return GridManager.Instance.grid.GetCellCenterWorld(new Vector3Int(node.gridX, node.gridY, 0));
                 }
            }
        }
        return Vector3.zero; // Return zero if no point is found
    }
}
