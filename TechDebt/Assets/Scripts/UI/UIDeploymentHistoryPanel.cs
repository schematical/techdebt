using System.Linq;
using UnityEngine;

namespace UI
{
    public class UIDeploymentHistoryPanel: UIPanel
    {

        void OnEnable()
        {
            GameManager.OnDeploymentChanged += HandleDeploymentChanged;
            Refresh();
        }

        void OnDisable()
        {
            GameManager.OnDeploymentChanged -= HandleDeploymentChanged;
        }

        private void HandleDeploymentChanged(DeploymentBase deployment, DeploymentBase.DeploymentState prevState)
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

            var deployments = GameManager.Instance.Deployments.ToList();
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