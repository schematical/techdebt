using System.Linq;
using UnityEngine;

namespace UI
{
    public class UIDeploymentHistoryPanel: UIPanel
    {
        public GameObject Content;

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
            foreach (Transform child in Content.transform)
            {
                Destroy(child.gameObject);
            }

            var deployments = GameManager.Instance.Deployments.ToList();
            deployments.Reverse();

            foreach (var deployment in deployments)
            {
                UITextArea textArea = GameManager.Instance.prefabManager.GetPrefab("UITextArea").GetComponent<UITextArea>();
                //TODO: Geminit Initialize it on the Content here.
                textArea.textArea.text = deployment.GetDescription();
            }
        }
    }
}