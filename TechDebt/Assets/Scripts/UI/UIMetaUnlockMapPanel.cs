using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI
{
    public class UIMetaUnlockMapNode : iUIMapNode
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public MapNodeDirection Direction { get; set; }
        public List<string> DependencyIds { get; set; }
        public int PrestigeCost { get; set; }
        
        public MapNodeState CurrentState
        {
            get
            {
                if (MetaGameManager.ProgressData.unlockedNodeIds.Contains(Id))
                    return MapNodeState.Unlocked;
                
                // If dependencies are met, it's Locked (purchasable), else MetaLocked
                bool dependenciesMet = DependencyIds == null || DependencyIds.Count == 0 || 
                                       DependencyIds.All(depId => MetaGameManager.ProgressData.unlockedNodeIds.Contains(depId));
                
                return dependenciesMet ? MapNodeState.Locked : MapNodeState.MetaLocked;
            }
        }

        public float GetProgress() => 0;

        public UnityEngine.Tilemaps.TileBase GetTile()
        {
            string tileId = "TechTreeLockedTile";
            switch (CurrentState)
            {
                case MapNodeState.MetaLocked:
                    tileId = "TechTreeLockedTile";
                    break;
                case MapNodeState.Locked:
                    tileId = "TechTreeUnlockedTile";
                    break;
                case MapNodeState.Active:
                    tileId = "TechTreeResearching";
                    break;
                case MapNodeState.Unlocked:
                    tileId = "TechTreeResearched";
                    break;
            }
            return GameManager.Instance.prefabManager.GetTile(tileId);
        }

        public void OnSelected(UIMapPanel panel)
        {
            // Logic handled by UIMetaUnlockMapPanel.UpdateDetailsArea
        }
    }

    public class UIMetaUnlockMapPanel : UIMapPanel
    {
        private List<UIMetaUnlockMapNode> _metaNodes = new List<UIMetaUnlockMapNode>();

        public override void PopulateNodes()
        {
            _metaNodes = GetMetaUnlockDefinitions();
            foreach (UIMetaUnlockMapNode node in _metaNodes)
            {
                _mapNodes.Add(new MapNodeView { Node = node });
            }
        }

        public override void UpdateDetailsArea()
        {
            CleanUp();
            
            UIPanelLine prestigeLine = AddLine<UIPanelLine>();
            prestigeLine.Add<UIPanelLineSectionText>().text.text = $"Prestige Points: {MetaGameManager.ProgressData.prestigePoints}";

            if (_selectedNode == null)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "Select a mapNode to unlock starting bonuses.";
                return;
            }

            UIMetaUnlockMapNode mapNode = (UIMetaUnlockMapNode)_selectedNode.Node;
            UIPanelLineSectionText header = AddLine<UIPanelLine>().Add<UIPanelLineSectionText>();
            header.h1(mapNode.DisplayName);
            
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = mapNode.Description;
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"\nCost: {mapNode.PrestigeCost} Prestige Points";

            if (mapNode.CurrentState == MapNodeState.Unlocked)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "\nALREADY UNLOCKED";
            }
            else if (mapNode.CurrentState == MapNodeState.Locked)
            {
                if (MetaGameManager.ProgressData.prestigePoints >= mapNode.PrestigeCost)
                {
                    AddButton("Unlock", () => {
                        PurchaseUnlock(mapNode);
                    });
                }
                else
                {
                    AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "\nNOT ENOUGH PRESTIGE POINTS";
                }
            }
            else
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "\nPREREQUISITES NOT MET";
            }
        }

        private void PurchaseUnlock(UIMetaUnlockMapNode mapNode)
        {
            MetaProgressData progress = MetaGameManager.LoadProgress();
            if (progress.prestigePoints >= mapNode.PrestigeCost)
            {
                progress.prestigePoints -= mapNode.PrestigeCost;
                progress.unlockedNodeIds.Add(mapNode.Id);
                MetaGameManager.SaveProgress(progress);
                Refresh();
            }
        }

        public override void Close(bool forceClose = false)
        {
            base.Close(forceClose);
            if (GameManager.Instance.State == GameManager.GameManagerState.MainMenu)
            {
                GameManager.Instance.UIManager.saveSlotDetailPanel.Show();
            }
        }

        private List<UIMetaUnlockMapNode> GetMetaUnlockDefinitions()
        {
            // Placeholder for actual definitions. 
            // In a real scenario, these might come from a ScriptableObject or MetaGameManager.
            return new List<UIMetaUnlockMapNode>
            {
                new UIMetaUnlockMapNode
                {
                    Id = "start-with-app-server",
                    DisplayName = "Ready to Go",
                    Description = "Start every run with the Application Server already unlocked.",
                    PrestigeCost = 10,
                    Direction = MapNodeDirection.Right,
                    DependencyIds = new List<string>()
                },
                new UIMetaUnlockMapNode
                {
                    Id = "start-with-whiteboard",
                    DisplayName = "Early Software",
                    Description = "Start every run with Software Basics already unlocked.",
                    PrestigeCost = 20,
                    Direction = MapNodeDirection.Right,
                    DependencyIds = new List<string> { "start-with-app-server" }
                }
            };
        }
    }
}