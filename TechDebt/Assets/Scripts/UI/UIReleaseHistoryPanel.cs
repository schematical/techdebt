using System.Linq;
using UnityEngine;

namespace UI
{
    public class UIReleaseHistoryPanel: UIPanel
    {

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
            foreach (Transform child in scrollContent.transform)
            {
                Destroy(child.gameObject);
            }

            var deployments = GameManager.Instance.Releases.ToList();
            deployments.Reverse();

            foreach (var deployment in deployments)
            {
                GameObject textAreaPrefab = GameManager.Instance.prefabManager.GetPrefab("UITextArea");
                UITextArea textArea = Instantiate(textAreaPrefab, scrollContent.transform).GetComponent<UITextArea>(); 
                textArea.textArea.text = deployment.GetDescription();
            }
        }
    }
}