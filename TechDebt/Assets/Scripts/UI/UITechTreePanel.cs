using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class UITechTreePanel : UIPanel
    {
        private List<UITechTreePanelItem> _techTreeItems = new List<UITechTreePanelItem>();

        public UITextArea metaUnlockTextArea;
        protected override void Update()
        {   
            base.Update();
            foreach (var item in _techTreeItems)
            {
                item.UpdateProgress();
            }
        }

        public void Refresh(Technology technology = null)
        {
            foreach (UITechTreePanelItem techTreePanelItem in _techTreeItems) 
            {
                techTreePanelItem.gameObject.SetActive(false);
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

        public override void Show()
        {
            GameManager.Instance.UIManager.Close();
            
            GameManager.OnTechnologyUnlocked += Refresh;
            GameManager.OnTechnologyResearchStarted += Refresh;
            base.Show();
            Refresh();
        }

        public virtual void Close()
        {
        
            GameManager.OnTechnologyUnlocked -= Refresh;
            GameManager.OnTechnologyResearchStarted -= Refresh;
            base.Close();
        }
    }
}