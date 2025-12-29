using Stats;
using UnityEngine;

namespace Events
{
    public class DeploymentEvent : EventBase
    {
        public DeploymentEvent()
        {
            EventStartText = "New deployments are available!";
            EventEndText = "Deployment opportunities have passed."; // Or perhaps an actual result
            Probility = 2; // Adjust probability as needed
        }

        public override void Apply()
        {
            base.Apply(); // Call base to trigger the alert
            Debug.Log("Applying DeploymentEvent - triggering creation of DeploymentTasks.");
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
    }
}
