using Stats;
using UnityEngine;

namespace Events
{
    // TODO: Move this to something like a hotfix event or something.
    public class DeploymentEvent : EventBase
    {
       
 
        public ReleaseBase Release;
        public DeploymentEvent()
        {
            EventStartText = "New deployments are available!";
            // EventEndText = "Deployment success."; // Or perhaps an actual result
            Probability = 10;
        }

     
        public override void Apply()
        {
            ReleaseBase.GlobalVersion += 1;
            Release = new ReleaseBase()
            {
                ServiceId = "monolith",
                Version = ReleaseBase.GlobalVersion,
            };
           
            EventStartText = $"Deployments {Release.GetVersionString()} is ready to go live!";
            base.Apply(); // Call base to trigger the alert
            
            GameManager.Instance.Releases.Add(Release);
            End();

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
                if (infra.GetComponent<ApplicationServer>() != null && infra.IsActive())
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
