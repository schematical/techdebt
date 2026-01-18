using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI
{
    public class UICurrentReleasePanel: MonoBehaviour
    {
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
            // Clear existing entries
            foreach (Transform child in scrollContent.transform)
            {
                Destroy(child.gameObject);
            }

            List<ReleaseBase> releases = GameManager.Instance.Releases.ToList();
            releases.Reverse();

            foreach (var release in releases)
            {
                GameObject textAreaPrefab = GameManager.Instance.prefabManager.GetPrefab("UIProgressBarPanel");
                UITextArea textArea = Instantiate(textAreaPrefab, scrollContent.transform).GetComponent<UITextArea>(); 
                textArea.textArea.text = release.GetDescription();
            }
        }
    }
}