using System.Linq;
using UnityEngine;

namespace UI
{
    public class UIReleaseHistoryPanel: UIPanel
    {

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