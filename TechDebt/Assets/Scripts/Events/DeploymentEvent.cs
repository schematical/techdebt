using Stats;
using UnityEngine;

namespace Events
{
    public class DeploymentEvent : EventBase
    {
 
        public DeploymentBase Deployment;
        public DeploymentEvent()
        {
            EventStartText = "New deployments are available!";
            EventEndText = "Deployment success."; // Or perhaps an actual result
            Probability = 10;
        }

     
        public override void Apply()
        {
            DeploymentBase.GlobalVersion += 1;
            Deployment = new DeploymentBase()
            {
                ServiceId = "monolith",
                Version = DeploymentBase.GlobalVersion,
            };
           
            EventStartText = $"Deployments {Deployment.GetVersionString()} is ready to go live!";
            base.Apply(); // Call base to trigger the alert
            
            GameManager.Instance.Deployments.Add(Deployment);
            
         
        }

        public override bool IsPossible()
        {
            // Only allow this event if there are operational servers to deploy to
            // and there isn't another DeploymentEvent currently active.
            if (GameManager.Instance.CurrentEvents.Contains(this))
            {
                return false;
            }
            bool hasOperationalServers = false;
            foreach (var infra in GameManager.Instance.ActiveInfrastructure)
            {
                if (infra.GetComponent<Server>() != null && infra.IsActive())
                {
                    hasOperationalServers = true;
                    break;
                }
            }

            return hasOperationalServers;
        }

     

        public override bool IsOver()
        {
            return true;
        }
    }
}
