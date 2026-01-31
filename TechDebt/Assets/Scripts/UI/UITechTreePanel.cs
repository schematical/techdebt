using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class UITechTreePanel : UIPanel
    {
        private List<UITechTreePanelItem> _techTreeItems = new List<UITechTreePanelItem>();

        public UITextArea metaUnlockTextArea;
        void Update()
        {
            foreach (var item in _techTreeItems)
            {
                item.UpdateProgress();
            }
        }

        public void Refresh(Technology technology = null)
        {
            foreach (UITechTreePanelItem techTreePanelItem in _techTreeItems) 
            {
                Destroy(techTreePanelItem.gameObject);
            }

            _techTreeItems.Clear();

            if (GameManager.Instance == null || GameManager.Instance.AllTechnologies == null) return;

            GameObject textAreaPrefab = GameManager.Instance.prefabManager.GetPrefab("UITextArea");
            if (textAreaPrefab == null)
            {
                Debug.LogError("UITextArea prefab not found. Cannot create tech panel content.");
                return;
            }

            foreach (var tech in GameManager.Instance.AllTechnologies)
            {
                Technology localTech = tech; // Create a local copy for the closure
                if (localTech.CurrentState == Technology.State.MetaLocked)
                {
                    continue;
                }

                UITechTreePanelItem techTreePanelItem =
                    GameManager.Instance.prefabManager
                        .Create("UITechTreePanelItem", Vector3.zero, scrollContent.transform)
                        .GetComponent<UITechTreePanelItem>();

                techTreePanelItem.Initialize(localTech);
                _techTreeItems.Add(techTreePanelItem);
            }
            metaUnlockTextArea.transform.SetAsLastSibling();
        }

        public void Show()
        {
            GameManager.Instance.UIManager.Close();
            gameObject.SetActive(true);
            GameManager.OnTechnologyUnlocked += Refresh;
            GameManager.OnTechnologyResearchStarted += Refresh;
            Refresh();
        }

        public void Close()
        {
            GameManager.OnTechnologyUnlocked -= Refresh;
            GameManager.OnTechnologyResearchStarted -= Refresh;
            gameObject.SetActive(false);
        }
    }
}