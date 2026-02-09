using System.Collections.Generic;
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

            foreach (var release in releases)
            {
                Color color = Color.white;
                switch (release.State) 
                {
                    case ReleaseBase.ReleaseState.Failed:
                    case ReleaseBase.ReleaseState.DeploymentCompleted:
                        if (ProgressBarPanels.ContainsKey(release.GetVersionString()))
                        {
                            color = new Color(1f, 1f, 1f, 0.5f);
                            ProgressBarPanels[release.GetVersionString()].gameObject.SetActive(false);
                            // Destroy(ProgressBarPanels[release.GetVersionString()].gameObject);
                        }
                    continue;
                    case ReleaseBase.ReleaseState.DeploymentRewardReady:
                    case ReleaseBase.ReleaseState.DeploymentReady:
                    case ReleaseBase.ReleaseState.DeploymentInProgress:
                        color =  Color.purple;
                        break;
                }
                if (!ProgressBarPanels.ContainsKey(release.GetVersionString()))
                {
                    GameObject progressBarGo = GameManager.Instance.prefabManager.Create("UIProgressBarPanel", Vector3.zero, scrollContent.transform);
                    ProgressBarPanels[release.GetVersionString()] = progressBarGo.GetComponent<UIProgressBarPanel>(); 
                }
              
                ProgressBarPanels[release.GetVersionString()].Text.text = release.GetDescription();
                ProgressBarPanels[release.GetVersionString()].SetProgress(release.CurrentProgress / release.RequiredProgress, color);
            }

            
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
              
                ProgressBarPanels["tech"].Text.text = "Researching: " + tech.DisplayName;
                ProgressBarPanels["tech"].SetProgress(tech.CurrentResearchProgress /tech.ResearchPointCost, Color.blue);
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
}