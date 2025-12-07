// NPCDevOps.cs
using UnityEngine;
using System.Collections;
using System.Linq;

public class NPCDevOps : NPCBase
{
    public enum State { Idle, Moving, Building }
    public State CurrentState { get; private set; } = State.Idle;

    public NPCDevOpsData Data { get; private set; }
    private InfrastructureInstance buildTarget;
    
    private Coroutine idleCoroutine;
    public float wanderRadius = 10f;


    public void Initialize(NPCDevOpsData data)
    {
        Data = data;
        gameObject.name = $"NPCDevOps_{Data.Name}";
    }

    public void OnPlayPhaseStart()
    {
        TryToFindWork();
    }

    private void TryToFindWork()
    {
        if (CurrentState != State.Idle) return;

        // Priority 1: Find a planned infrastructure to build
        buildTarget = FindClosestPlannedInfrastructure();
        if (buildTarget != null)
        {
            if (idleCoroutine != null)
            {
                StopCoroutine(idleCoroutine);
                idleCoroutine = null;
            }
            
            CurrentState = State.Moving;
            Debug.Log($"{Data.Name} is moving to build {buildTarget.data.DisplayName}.");
            MoveTo(buildTarget.transform.position);
            StartCoroutine(BuildingRoutine());
        }
        else
        {
            // Priority 2: If no work, start wandering
            if (idleCoroutine == null)
            {
                idleCoroutine = StartCoroutine(Wander());
            }
        }
    }

    private IEnumerator BuildingRoutine()
    {
        // Wait until NPC reaches the destination
        while (Vector3.Distance(transform.position, buildTarget.transform.position) > 0.1f)
        {
            yield return null;
        }

        // Arrived at build site
        CurrentState = State.Building;
        Debug.Log($"{Data.Name} started building {buildTarget.data.DisplayName}. Time: {buildTarget.data.BuildTime}s");
        yield return new WaitForSeconds(buildTarget.data.BuildTime);

        // Finished building
        buildTarget.SetState(InfrastructureData.State.Operational);
        GameManager.Instance.NotifyInfrastructureBuilt(buildTarget);
        Debug.Log($"{Data.Name} finished building {buildTarget.data.DisplayName}!");
        GameManager.Instance.NotifyDailyCostChanged();

        // Add the new server to the list of available servers if it is one
        var serverComponent = buildTarget.GetComponent<Server>();
        if(serverComponent != null && !GameManager.AllServers.Contains(serverComponent))
        {
            GameManager.AllServers.Add(serverComponent);
        }

        CurrentState = State.Idle;
        buildTarget = null;

        // After finishing a build, look for more work immediately
        TryToFindWork();
    }

    private InfrastructureInstance FindClosestPlannedInfrastructure()
    {
        return FindObjectsOfType<InfrastructureInstance>()
            .Where(i => i.data.CurrentState == InfrastructureData.State.Planned)
            .OrderBy(i => Vector3.Distance(transform.position, i.transform.position))
            .FirstOrDefault();
    }

    private IEnumerator Wander()
    {
        while (true)
        {
            CurrentState = State.Idle;
            Debug.Log($"{Data.Name} is wandering.");

            Vector3 randomPoint;
            if (GetRandomWalkablePoint(transform.position, wanderRadius, out randomPoint))
            {
                CurrentState = State.Moving;
                MoveTo(randomPoint);

                // Wait until the agent is close to the destination
                while (isMoving)
                {
                    yield return null;
                }
            }

            CurrentState = State.Idle;
            yield return new WaitForSeconds(5f);
        }
    }

    private bool GetRandomWalkablePoint(Vector3 origin, float radius, out Vector3 result)
    {
        for (int i = 0; i < 30; i++) // Try 30 times to find a point
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += origin;
            
            // Check if the point is walkable via the GridManager
            Node node = GridManager.Instance.NodeFromWorldPoint(randomDirection);
            if (node != null && node.isWalkable)
            {
                result = randomDirection;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }
}