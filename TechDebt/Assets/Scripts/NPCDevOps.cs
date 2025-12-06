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
            CurrentState = State.Moving;
            Debug.Log($"{Data.Name} is moving to build {buildTarget.data.DisplayName}.");
            MoveTo(buildTarget.transform.position);
            StartCoroutine(BuildingRoutine());
            return;
        }

        // Priority 2: Default behavior (move to a random server)
        FindAndMoveToRandomServer();
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

    private void FindAndMoveToRandomServer()
    {
        if (GameManager.AllServers != null && GameManager.AllServers.Count > 0)
        {
            CurrentState = State.Moving;
            Server randomServer = GameManager.AllServers[Random.Range(0, GameManager.AllServers.Count)];
            MoveTo(randomServer.transform.position);
        }
    }
}