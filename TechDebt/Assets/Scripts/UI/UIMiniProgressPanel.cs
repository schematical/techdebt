/*using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI
{
    public class UIMiniProgressPanel: MonoBehaviour
    {
        public Dictionary<string, UIProgressBarPanel> ProgressBarPanels = new Dictionary<string, UIProgressBarPanel>();
        public Transform scrollContent;
        void OnEnable()
        {
            GameManager.OnReleaseChanged += HandleReleaseChanged;
            Refresh();
        }

        void OnDisable()
        {
            GameManager.OnReleaseChanged -= HandleReleaseChanged;
        }

        private void HandleReleaseChanged(ReleaseBase release, ReleaseBase.ReleaseState prevState)
        {
            Refresh();
        }

        private void Refresh()
        {

            if (GameManager.Instance == null)
            {
                return;
            }
            List<ReleaseBase> releases = GameManager.Instance.Releases.ToList();
            releases.Reverse();

            ReleaseBase release = releases.Find(release => release.State != ReleaseBase.ReleaseState.DeploymentCompleted);
            if (release == null)
            {
                ProgressBarPanels["release"].gameObject.SetActive(false);
                return;
            }
            if (!ProgressBarPanels.ContainsKey("release"))
            {
                GameObject progressBarGo = GameManager.Instance.prefabManager.Create("UIProgressBarPanel", Vector3.zero, scrollContent.transform);
                progressBarGo.SetActive(true);
                ProgressBarPanels["release"] = progressBarGo.GetComponent<UIProgressBarPanel>(); 
            }
          
            ProgressBarPanels["release"].Text.text = release.GetDescription();
            ProgressBarPanels["release"].SetProgress(release.CurrentProgress / release.RequiredProgress, Color.green);
        

            
        }

        void Update()
        {
            Technology tech = GameManager.Instance.CurrentlyResearchingTechnology;
            if (tech != null && tech.CurrentState == Technology.State.Researching)
            {
         
                if (!ProgressBarPanels.ContainsKey("tech"))
                {
                    GameObject progressBarGo = GameManager.Instance.prefabManager.Create("UIProgressBarPanel", Vector3.zero, scrollContent.transform);
                    ProgressBarPanels["tech"] = progressBarGo.GetComponent<UIProgressBarPanel>(); 
                  
                }
                else
                {
                    ProgressBarPanels["tech"].gameObject.SetActive(true); 
                }
              
                ProgressBarPanels["tech"].Text.text = "Researching: " + tech.DisplayName;
                ProgressBarPanels["tech"].SetProgress(tech.CurrentResearchProgress /tech.ResearchTime, Color.blue);
            }
            else
            {
                if (ProgressBarPanels.ContainsKey("tech"))
                {
                    ProgressBarPanels["tech"].gameObject.SetActive(false);
                }
            }
        }
    }
}*/