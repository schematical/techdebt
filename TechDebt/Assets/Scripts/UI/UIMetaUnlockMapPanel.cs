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
            Debug.Log($"SwitchTab {index} - _tabs.Count: {_tabs.Count}");
            _currentTabIndex = index;
            _selectedNode = null;
            Refresh();
        }

        // Methods to be called from UI buttons in Editor
        public void SwitchToTechnology() => SwitchTab(0);
        public void SwitchToBonuses() => SwitchTab(1);
        public void SwitchToOrgChart() => SwitchTab(2);

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