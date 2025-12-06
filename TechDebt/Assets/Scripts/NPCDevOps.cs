// NPCDevOps.cs
using UnityEngine;

public class NPCDevOps : NPCBase
{
    void Start()
    {
        // Find a random server to move to.
        if (GameManager.AllServers != null && GameManager.AllServers.Count > 0)
        {
            Server randomServer = GameManager.AllServers[Random.Range(0, GameManager.AllServers.Count)];
            Debug.Log($"{gameObject.name} is choosing to pathfind to server at {randomServer.transform.position}.");
            MoveTo(randomServer.transform.position);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} could not find any servers to move to.");
        }
    }
}