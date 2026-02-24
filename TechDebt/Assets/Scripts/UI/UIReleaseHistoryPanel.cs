using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI
{
    public class UIReleaseHistoryPanel: UIPanel
    {
        public Dictionary<string, UITextArea> textAreas = new Dictionary<string, UITextArea>();
        public override void Show()
        {
            base.Show();
            GameManager.OnReleaseChanged += HandleReleaseChanged;
            Refresh();
        }

        public override void Close(bool forceClose = false)
        {
            base.Close(forceClose);
            GameManager.OnReleaseChanged -= HandleReleaseChanged;
        }

        private void HandleReleaseChanged(ReleaseBase release, ReleaseBase.ReleaseState prevState)
        {
            Refresh();
        }

        private void Refresh()
        {
            // Clear existing entries
           

            List<ReleaseBase> releases = GameManager.Instance.Releases.ToList();
            releases.Reverse();

            foreach (ReleaseBase release in releases)
            {
                if (!textAreas.ContainsKey(release.GetVersionString()))
                {
                    textAreas[release.GetVersionString()] = GameManager.Instance.prefabManager.Create("UITextArea", Vector3.zero, scrollContent.transform).GetComponent<UITextArea>();
                }
                textAreas[release.GetVersionString()] .textArea.text = release.GetDescription();
            }
        }
    }
}