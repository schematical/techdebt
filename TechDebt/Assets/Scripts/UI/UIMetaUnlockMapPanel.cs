using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIMetaUnlockMapPanel : UIMapPanel
    {
        private List<UIMetaUnlockMapTabBase> _tabs;
        private int _currentTabIndex = 0;
        public Button CloseButton2;

        public Transform metaUnlockMapTabs;

        public List<UIButton> tabButtons = new();

        private Vector2 _initialAnchorMin;
        private Vector2 _initialAnchorMax;

        protected override void Awake()
        {
            base.Awake();
            _initialAnchorMin = rectTransform.anchorMin;
            _initialAnchorMax = rectTransform.anchorMax;
        }

        protected override void Update()
        {
            // Intercept the end of the Closing animation to prevent UIGameObject from deactivating the panel.
            if (panelState == UIState.Closing && canvasGroup.alpha < 0.05f)
            {
                panelState = UIState.Closed;
                
                Vector2 offset = GetSlideOffset();
                rectTransform.anchorMin = _initialAnchorMin + offset;
                rectTransform.anchorMax = _initialAnchorMax + offset;
                canvasGroup.alpha = 0;
            }

            base.Update();
        }

        private Vector2 GetSlideOffset()
        {
            switch (slideDirection)
            {
                case SlideDirection.Left: return new Vector2(1, 0);
                case SlideDirection.Right: return new Vector2(-1, 0);
                case SlideDirection.Up: return new Vector2(0, -1);
                case SlideDirection.Down: return new Vector2(0, 1);
                default: return Vector2.zero;
            }
        }

        public void SetupTabs()
        {
            _tabs = new List<UIMetaUnlockMapTabBase> 
            {
       
                new UIMetaUnlockBonusesTab(),
                new UIMetaUnlockTechnologyTab(),
                new UIMetaUnlockOrgChartTab()
            };
            foreach (UIMetaUnlockMapTabBase tab in _tabs)
            {
                tab.Initialize(this);
            }

            CreateTabButtons();
        }

        private void CreateTabButtons()
        {
            foreach (UIButton tabButton in tabButtons)
            {
                if (tabButton != null) Destroy(tabButton.gameObject);
            }
            tabButtons.Clear();
            
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
            uiBtn.button.onClick.RemoveAllListeners();
            uiBtn.button.onClick.AddListener(() => SwitchTab(index));
            tabButtons.Add(uiBtn);
        }

        public override void Show()
        {
            // We manually implement Show logic to avoid base.Show() triggering an immediate SlideIn()
            // of the entire detail panel. We want the object active but the UI hidden.
            bool isInitialShow = !IsOpen();
            
            if (runUICloseOnShow)
            {
                GameManager.Instance.UIManager.CloseSideBars();
            }

            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(true);
            }

            if (CloseButton2 != null)
            {
                CloseButton2.onClick.RemoveAllListeners();
                CloseButton2.onClick.AddListener(SlideOut);
            }

            gameObject.SetActive(true);
            panelState = UIState.Closed;
            if (canvasGroup != null) canvasGroup.alpha = 0;
            hasUpdateThisFrame = true;

            // UIMapPanel setup logic
            GameManager.Instance.UIManager.ForcePause();
            GameManager.Instance.UIManager.victoryConditionListPanel.Close();
            grid.gameObject.SetActive(true);
            
            // The tabs should be visible immediately at the top
            if (metaUnlockMapTabs != null)
            {
                metaUnlockMapTabs.gameObject.SetActive(true);
                // If the tabs container itself has a UIGameObject, ensure it's shown.
                // Assuming it's static or managed separately.
            }
            
            SetupTabs();
            Refresh();
            
            GameManager.Instance.cameraController.DisableCameraInput();
            
            SwitchTab(0, isInitialShow);
        }

        protected override void SelectNode(MapNodeView nodeView)
        {
            base.SelectNode(nodeView);
            
            // Slide in the detail panel only when a node is selected
            if (panelState == UIState.Closed)
            {
                SlideIn();
            }
        }

        public void SwitchTab(int index, bool zoomToFit = false)
        {
            _currentTabIndex = index;
            _selectedNode = null;
            Refresh();
            CenterTilemapOnCamera(zoomToFit);
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
            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(false);
            }

            if (forceClose)
            {
                // Replicate UIMapPanel.Close force logic but skip SetActive(false) on this GameObject
                foreach (MapNodeView nodeView in _mapNodes)
                {
                    if (nodeView.LabelInstance != null)
                    {
                        nodeView.LabelInstance.gameObject.SetActive(false);
                    }
                }

                _mapNodes.Clear();
                if (nodeTilemap != null) nodeTilemap.ClearAllTiles();
                if (connectorTilemap != null) connectorTilemap.ClearAllTiles();
                if (backgroundTilemap != null) backgroundTilemap.ClearAllTiles();
                
                if (grid != null) grid.gameObject.SetActive(false);

                panelState = UIState.Closed;
                Vector2 offset = GetSlideOffset();
                rectTransform.anchorMin = _initialAnchorMin + offset;
                rectTransform.anchorMax = _initialAnchorMax + offset;
                canvasGroup.alpha = 0;
            }
            else
            {
                base.Close(forceClose);
            }
            
            if (metaUnlockMapTabs != null) metaUnlockMapTabs.gameObject.SetActive(false);
            
            if (GameManager.Instance.State == GameManager.GameManagerState.MainMenu)
            {
                GameManager.Instance.UIManager.saveSlotDetailPanel.Show();
            }
        }
    }
}