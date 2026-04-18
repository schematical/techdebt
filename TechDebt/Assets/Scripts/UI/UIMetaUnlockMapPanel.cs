using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UIMetaUnlockMapPanel : UIMapPanel
    {
        private List<UIMetaUnlockMapTabBase> _tabs;
        private int _currentTabIndex = 0;

        public Transform metaUnlockMapTabs;

     

        public void SetupTabs()
        {
            Debug.Log("SetupTabs");
            _tabs = new List<UIMetaUnlockMapTabBase> 
            {
                new UIMetaUnlockTechnologyTab(),
                new UIMetaUnlockBonusesTab(),
                new UIMetaUnlockOrgChartTab()
            };
            foreach (var tab in _tabs)
            {
                tab.Initialize(this);
            }

            CreateTabButtons();
        }

        private void CreateTabButtons()
        {
            // Clean up existing buttons
            foreach (Transform child in metaUnlockMapTabs)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < _tabs.Count; i++)
            {
                int index = i; // Local copy for closure
                AddTabButton(_tabs[i].TabName, index);
            }
        }

        private void AddTabButton(string label, int index)
        {
            GameObject btnGO = GameManager.Instance.prefabManager.Create("UIButton", Vector3.zero, metaUnlockMapTabs);
            UIButton uiBtn = btnGO.GetComponent<UIButton>();
            uiBtn.buttonText.text = label;
            uiBtn.button.onClick.AddListener(() => SwitchTab(index));
        }

        public override void Show()
        {
            base.Show();
            
            metaUnlockMapTabs.gameObject.SetActive(true);
            if (_tabs == null || _tabs.Count == 0)
            {
                SetupTabs();
            }
            
            // Default to first tab (Technology)
            SwitchTab(0);
        }

        public void SwitchTab(int index)
        {
            _currentTabIndex = index;
            _selectedNode = null;
            Refresh();
            CenterTilemapOnCamera();
        }
        

        public override void PopulateNodes()
        {
            if (_tabs == null || _currentTabIndex >= _tabs.Count) return;
            _tabs[_currentTabIndex].PopulateNodes(_mapNodes);
        }

        public override void UpdateDetailsArea()
        {
            if (_tabs == null || _currentTabIndex >= _tabs.Count) return;
            _tabs[_currentTabIndex].UpdateDetailsArea();
        }

        protected override bool IsNodeVisible(MapNodeView nodeView)
        {
            return true;
        }

        public override void Close(bool forceClose = false)
        {
            base.Close(forceClose);
            
            if (metaUnlockMapTabs != null) metaUnlockMapTabs.gameObject.SetActive(false);
            
            if (GameManager.Instance.State == GameManager.GameManagerState.MainMenu)
            {
                GameManager.Instance.UIManager.saveSlotDetailPanel.Show();
            }
        }
    }
}