using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI
{
    public class MetaUnlockNode : iMapNode
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
            // Logic handled by UIMetaUnlockPanel.UpdateDetailsArea
        }
    }

    public class UIMetaUnlockPanel : UIMapPanel
    {
        private List<MetaUnlockNode> _metaNodes = new List<MetaUnlockNode>();

        public override void PopulateNodes()
        {
            _metaNodes = GetMetaUnlockDefinitions();
            foreach (MetaUnlockNode node in _metaNodes)
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
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "Select a node to unlock starting bonuses.";
                return;
            }

            MetaUnlockNode node = (MetaUnlockNode)_selectedNode.Node;
            UIPanelLineSectionText header = AddLine<UIPanelLine>().Add<UIPanelLineSectionText>();
            header.h1(node.DisplayName);
            
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = node.Description;
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"\nCost: {node.PrestigeCost} Prestige Points";

            if (node.CurrentState == MapNodeState.Unlocked)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "\nALREADY UNLOCKED";
            }
            else if (node.CurrentState == MapNodeState.Locked)
            {
                if (MetaGameManager.ProgressData.prestigePoints >= node.PrestigeCost)
                {
                    AddButton("Unlock", () => {
                        PurchaseUnlock(node);
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

        private void PurchaseUnlock(MetaUnlockNode node)
        {
            MetaProgressData progress = MetaGameManager.LoadProgress();
            if (progress.prestigePoints >= node.PrestigeCost)
            {
                progress.prestigePoints -= node.PrestigeCost;
                progress.unlockedNodeIds.Add(node.Id);
                MetaGameManager.SaveProgress(progress);
                Refresh();
            }
        }

        private List<MetaUnlockNode> GetMetaUnlockDefinitions()
        {
            // Placeholder for actual definitions. 
            // In a real scenario, these might come from a ScriptableObject or MetaGameManager.
            return new List<MetaUnlockNode>
            {
                new MetaUnlockNode
                {
                    Id = "start-with-app-server",
                    DisplayName = "Ready to Go",
                    Description = "Start every run with the Application Server already unlocked.",
                    PrestigeCost = 10,
                    Direction = MapNodeDirection.Right,
                    DependencyIds = new List<string>()
                },
                new MetaUnlockNode
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