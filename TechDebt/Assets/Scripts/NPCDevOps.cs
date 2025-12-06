// NPCDevOps.cs
using UnityEngine;

public class NPCDevOps : NPCBase
{
    public NPCDevOpsData Data { get; private set; }

    public void Initialize(NPCDevOpsData data)
    {
        Data = data;
        gameObject.name = $"NPCDevOps_{Data.Name}";
    }

    // This method is called by the GameLoopManager at the beginning of the Play phase.
    public void OnPlayPhaseStart()
    {
        FindAndMoveToRandomServer();
    }

    private void FindAndMoveToRandomServer()
    {
        if (GameManager.AllServers != null && GameManager.AllServers.Count > 0)
        {
            Server randomServer = GameManager.AllServers[Random.Range(0, GameManager.AllServers.Count)];
            Debug.Log($"{gameObject.name} (Salary: ${Data.DailyCost}) is moving to server at {randomServer.transform.position}.");
            MoveTo(randomServer.transform.position);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} could not find any servers to move to.");
        }
    }
}
