using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI
{
    public class UICurrentReleasePanel: MonoBehaviour
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
                            // ProgressBarPanels[release.GetVersionString()].gameObject.SetActive(false);
                        }
                    continue;
                    case ReleaseBase.ReleaseState.DeploymentReady:
                    case ReleaseBase.ReleaseState.DeploymentInProgress:
                        color =  Color.blue;
                        break;
                }
                if (!ProgressBarPanels.ContainsKey(release.GetVersionString()))
                {
                    GameObject progrssBarPrefab = GameManager.Instance.prefabManager.GetPrefab("UIProgressBarPanel");
                    ProgressBarPanels[release.GetVersionString()] = Instantiate(progrssBarPrefab, scrollContent.transform).GetComponent<UIProgressBarPanel>(); 
                }
              
                ProgressBarPanels[release.GetVersionString()].Text.text = release.GetDescription();
                ProgressBarPanels[release.GetVersionString()].SetProgress(release.CurrentProgress / release.RequiredProgress, color);
            }
        }
    }
}