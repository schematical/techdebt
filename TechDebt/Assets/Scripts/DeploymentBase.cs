
public class DeploymentBase
{
    public static int GlobalVersion = 0;
    public enum DeploymentState
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
    public DeploymentState State { get; set; } = DeploymentState.InProgress;
    public DeploymentBase()
    {
        SetState(DeploymentState.InProgress);
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
        State  = DeploymentState.Completed;
        GameManager.Instance.UIManager.ShowAlert($"Deployment {GetVersionString()} Complete");
        return true;
    }

    public void SetState(DeploymentState state)
    {
        DeploymentState prevState = State;
        
        State = state;
        GameManager.Instance.InvokeDeploymentChanged(this, prevState);
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
