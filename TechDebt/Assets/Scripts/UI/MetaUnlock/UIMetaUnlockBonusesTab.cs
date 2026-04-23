using System.Collections.Generic;
using System.Linq;
using MetaChallenges;
using UnityEngine;

namespace UI
{
    public class UIMetaUnlockBonusesTab : UIMetaUnlockMapTabBase
    {
        public override string TabName => "Bonuses";

        public override void PopulateNodes(List<UIMapPanel.MapNodeView> mapNodes)
        {
            foreach (UIMetaUnlockMapNode node in GetMetaUnlockDefinitions())
            {
                SetNodeState(node);
                mapNodes.Add(new UIMapPanel.MapNodeView { Node = node });
            }
        }

        public override void UpdateDetailsArea()
        {
            _panel.CleanUp();
            
            UIPanelLine prestigeLine = _panel.AddLine<UIPanelLine>();
            prestigeLine.Add<UIPanelLineSectionText>().text.text = $"Vested Shares: {GetAvailablePrestigePoints()}";

            UIMapPanel.MapNodeView selectedNode = _panel.GetSelectedNode();
            if (selectedNode == null)
            {
                _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "Select a node to allocate Vested Shares.";
                return;
            }

            UIMetaUnlockMapNode mapNode = (UIMetaUnlockMapNode)selectedNode.Node;
            UIPanelLineSectionText header = _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>();
            header.h1(mapNode.DisplayName);

            _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = mapNode.Description;
            _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"\nCost: {mapNode.PrestigeCost} Vested Shares";

            if (mapNode.DependencyIds != null && mapNode.DependencyIds.Count > 0)
            {
                _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "\nRequirements:";
                foreach (string depId in mapNode.DependencyIds)
                {
                    string depName = depId;
                    UIMetaUnlockMapNode depNode = GetNodeById(depId);
                    if (depNode != null) depName = depNode.DisplayName;

                    bool met = false;
                    if (depNode != null) met = MetaGameManager.IsPrestigePointAllocationLeveledUp(depNode.AllocationId, depNode.Level);
                    
                    string status = met ? "<color=green>(MET)</color>" : "<color=red>(NOT MET)</color>";
                    _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $" - {depName} {status}";
                }
            }

            bool isEquipped = MetaGameManager.IsPrestigePointAllocationLeveledUp(mapNode.AllocationId, mapNode.Level);

            if (isEquipped)
            {
                _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "\nSTATUS: ALLOCATED (START UNLOCKED)";
                
                MetaProgressData progress = MetaGameManager.GetProgress();
                List<MetaPrestigePointAllocation> allocatedDependents = progress.prestigePointAllocations.FindAll(r => {
                    UIMetaUnlockMapNode node = GetNodeById(r.Id);
                    return node != null && node.DependencyIds != null && node.DependencyIds.Contains(mapNode.Id);
                });

                if (allocatedDependents.Count > 0)
                {
                    string depNames = string.Join(", ", allocatedDependents.Select(d => {
                        UIMetaUnlockMapNode n = GetNodeById(d.Id);
                        return n != null ? n.DisplayName : d.Id;
                    }));
                    _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"<color=orange>Warning: Unallocating this will also refund dependents: {depNames}</color>";
                }

                _panel.AddButton("Unallocate (Refund)", () => {
                    UnallocateRecursive(mapNode);
                    _panel.Refresh();
                });
            }
            else
            {
                if (mapNode.CurrentState == MapNodeState.Locked)
                {
                    if (GetAvailablePrestigePoints() >= mapNode.PrestigeCost)
                    {
                        _panel.AddButton("Allocate", () => {
                            MetaGameManager.UpdatePrestigePointAllocation( mapNode.AllocationId, mapNode.Level, mapNode.PrestigeCost);
                            _panel.Refresh();
                        });
                    }
                    else
                    {
                        _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "\nNOT ENOUGH VESTED SHARES";
                    }
                }
                else
                {
                    _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "\nPREREQUISITES NOT MET";
                }
            }
        }

    
        private List<UIMetaUnlockMapNode> GetMetaUnlockDefinitions()
        {
            List<UIMetaUnlockMapNode> nodes = new List<UIMetaUnlockMapNode>();

            float[] moneyValues = { 50, 100, 200, 400, 800, 1600 };
            for (int i = 1; i <= 6; i++)
            {
                UIMetaUnlockMapNode node = new UIMetaUnlockMapNode
                {
                    Id = $"money-{i}",
                    AllocationId = "money",
                    Level = i,
                    DisplayName = $"Budget Bonus {i}",
                    Description = $"Start each run with an additional ${moneyValues[i-1]}.",
                    PrestigeCost = i,
                    Direction = MapNodeDirection.Up,
                    DependencyIds = i == 1 ? new List<string>() : new List<string> { $"money-{i-1}" }
                };
                nodes.Add(node);
            }

            for (int i = 1; i <= 6; i++)
            {
                UIMetaUnlockMapNode node = new UIMetaUnlockMapNode
                {
                    Id = $"reroll-{i}",
                    AllocationId = "reroll",
                    Level = i,
                    DisplayName = $"ReRoll {i}",
                    Description = $"Gain an additional Re-Roll (Total: {i}).",
                    PrestigeCost = i,
                    Direction = MapNodeDirection.Down,
                    DependencyIds = i == 1 ? new List<string> { "money-1" } : new List<string> { $"reroll-{i - 1}" }
                };
                nodes.Add(node);
            }

            for (int i = 1; i <= 6; i++)
            {
                nodes.Add(new UIMetaUnlockMapNode
                {
                    Id = $"banish-{i}",
                    AllocationId = "banish",
                    Level = i,
                    DisplayName = $"Banish Level {i}",
                    Description = $"Gain an additional Banish (Total: {i}).",
                    PrestigeCost = i,
                    Direction = MapNodeDirection.Down,
                    DependencyIds = i == 1 ? new List<string> { "money-1" } : new List<string> { $"banish-{i-1}" }
                });
            }
            
            for (int i = 1; i <= 6; i++)
            {
                nodes.Add(new UIMetaUnlockMapNode
                {
                    Id = $"training-program-{i}",
                    AllocationId = "training-program",
                    Level = i,
                    DisplayName = $"Training Program {i}",
                    Description = $"Team Members are more likely to get rarer level ups",
                    PrestigeCost = i,
                    Direction = MapNodeDirection.Left,
                    DependencyIds = i == 1 ? new List<string> { "money-1" } : new List<string> { $"training-program-{i-1}" },
                });
            }

            return nodes;
        }
    }
}