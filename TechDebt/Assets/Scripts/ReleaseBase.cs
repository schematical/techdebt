
public class ReleaseBase
{
    /*
     *
     * MATTS NOTES:
     * DEPLOYMENT EFFECTS
     * - Decreased Image Load Costs
     * - Security bonuses
     * - Network Packet Load Costs
     * - A/B testing, Sales Page updates, Shopping cart enhancement, Daily Income Bonus
     * - Disk Space Bonus
     * - Documentation - Makes it easier for new NPCS to learn infra.
     * - Cross Training - Requires 2 NPCS or more - Enhances NPCs knowledge, prevents knowledge silo events.
     */
    public static int GlobalVersion = 0;
    public enum ReleaseState
    {
        InProgress,
        Completed,
        Failed
    }
    public string ServiceId { get; set; }
    public int Version  { get; set; }
    
    // TODO Create a hidden "Bug Count"
    // MinorBugs - that has a negative impact on the amount of money you make each day.
    // MajorBugs - Knock down the whole infrastructureInstance
    // TODO: Optimized score, gives a latency bonus or a load bonus
    public ReleaseState State { get; set; } = ReleaseState.InProgress;
    public ReleaseBase()
    {
        SetState(ReleaseState.InProgress);
    }


    public void QueueUpTask()
    {
        foreach (var infra in GameManager.Instance.ActiveInfrastructure)
        {
            Server server = infra.GetComponent<Server>();
            if (server != null && infra.data.CurrentState == InfrastructureData.State.Operational)
            {
                GameManager.Instance.AddTask(new DeploymentTask(server, this));
            }
        }
    }
    public string GetVersionString()
    {
        return $"0.{Version}.0";
    }
    public bool CheckIsOver()
    {
            
            
        foreach (var infra in GameManager.Instance.ActiveInfrastructure)
        {
            Server server = infra.GetComponent<Server>();
            if (
                server == null ||
                !infra.IsActive()
            )
            {
                continue;
            }
            if (
                infra.Version != GetVersionString()
            )
            {
                
                return false;
            }
        }
        State  = ReleaseState.Completed;
        GameManager.Instance.UIManager.ShowAlert($"Deployment {GetVersionString()} Complete");
        return true;
    }

    public void SetState(ReleaseState state)
    {
        ReleaseState prevState = State;
        
        State = state;
        GameManager.Instance.InvokeReleaseChanged(this, prevState);
    }

    public float GetDuration()
    {
        return 30;
    }

    public string GetDescription()
    {
        return $"{GetVersionString()} {State.ToString()}";
    }
}
