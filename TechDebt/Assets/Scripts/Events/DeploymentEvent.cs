using Stats;
using UnityEngine;

namespace Events
{
    public class DeploymentEvent : EventBase
    {
        public int Version { get; private set; } = 1;
        public DeploymentEvent()
        {
            EventStartText = "New deployments are available!";
            EventEndText = "Deployment success."; // Or perhaps an actual result
        }

        public string GetVersionString()
        {
            return $"0.{Version}.0";
        }
        public override void Apply()
        {
            Version += 1;
            EventStartText = $"Deployments {GetVersionString()} is ready to go live!";
            base.Apply(); // Call base to trigger the alert
 
            foreach (var infra in GameManager.Instance.ActiveInfrastructure)
            {
                Server server = infra.GetComponent<Server>();
                if (server != null && infra.data.CurrentState == InfrastructureData.State.Operational)
                {
                    GameManager.Instance.AddTask(new DeploymentTask(server, this));
                }
            }
        }

        public override bool IsPossible()
        {
            // Only allow this event if there are operational servers to deploy to
            // and there isn't another DeploymentEvent currently active.
            bool hasOperationalServers = false;
            foreach (var infra in GameManager.Instance.ActiveInfrastructure)
            {
                if (infra.GetComponent<Server>() != null && infra.data.CurrentState == InfrastructureData.State.Operational)
                {
                    hasOperationalServers = true;
                    break;
                }
            }

            return hasOperationalServers && !GameManager.Instance.CurrentEvents.Contains(this);
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
            EventEndText = $"Deployments {GetVersionString()} is live!";
            GameManager.Instance.EndEvent(this);
            return true;
        }
    }
}
