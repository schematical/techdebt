using System.Collections.Generic;
using System.Linq;
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
            
            MetaProgressData progress = MetaGameManager.LoadProgress();
            UIPanelLine prestigeLine = _panel.AddLine<UIPanelLine>();
            prestigeLine.Add<UIPanelLineSectionText>().text.text = $"Vested Shares: {progress.prestigePoints}";

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

                    bool met = MetaGameManager.IsResourceEquipped(mapNode.ResourceType, depId);
                    string status = met ? "<color=green>(MET)</color>" : "<color=red>(NOT MET)</color>";
                    _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $" - {depName} {status}";
                }
            }

            bool isEquipped = MetaGameManager.IsResourceEquipped(mapNode.ResourceType, mapNode.Id);

            if (isEquipped)
            {
                _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "\nSTATUS: ALLOCATED (START UNLOCKED)";
                
                List<MetaUnlockResource> allocatedDependents = progress.prestigePointAllocations.FindAll(r => {
                    if (r.Type != mapNode.ResourceType) return false;
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
                    UnallocateRecursive(mapNode.ResourceType, mapNode.Id, mapNode.PrestigeCost);
                    _panel.Refresh();
                });
            }
            else
            {
                if (mapNode.CurrentState == MapNodeState.Locked)
                {
                    if (progress.prestigePoints >= mapNode.PrestigeCost)
                    {
                        _panel.AddButton("Allocate", () => {
                            MetaGameManager.ToggleResourceEquip(mapNode.ResourceType, mapNode.Id, mapNode.PrestigeCost, mapNode.StatType, mapNode.Value);
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
                    ResourceType = MetaResourceType.GlobalStat,
                    Id = $"money-{i}",
                    DisplayName = $"Budget Bonus {i}",
                    Description = $"Start each run with an additional ${moneyValues[i-1]}.",
                    PrestigeCost = i,
                    Direction = MapNodeDirection.Up,
                    DependencyIds = i == 1 ? new List<string>() : new List<string> { $"money-{i-1}" },
                    StatType = StatType.Money,
                    Value = moneyValues[i-1]
                };
                nodes.Add(node);
            }

            for (int i = 1; i <= 6; i++)
            {
                UIMetaUnlockMapNode node = new UIMetaUnlockMapNode
                {
                    ResourceType = MetaResourceType.GlobalStat,
                    Id = $"reroll-{i}",
                    DisplayName = $"ReRoll {i}",
                    Description = $"Gain an additional Re-Roll (Total: {i}).",
                    PrestigeCost = i,
                    Direction = MapNodeDirection.Down,
                    DependencyIds = i == 1 ? new List<string> { "money-1" } : new List<string> { $"reroll-{i - 1}" },
                    StatType = StatType.Global_ReRolls,
                    Value = 1
                };
                nodes.Add(node);
            }

            for (int i = 1; i <= 6; i++)
            {
                nodes.Add(new UIMetaUnlockMapNode
                {
                    ResourceType = MetaResourceType.GlobalStat,
                    Id = $"banish-{i}",
                    DisplayName = $"Banish Level {i}",
                    Description = $"Gain an additional Banish (Total: {i}).",
                    PrestigeCost = i,
                    Direction = MapNodeDirection.Down,
                    DependencyIds = i == 1 ? new List<string> { "money-1" } : new List<string> { $"banish-{i-1}" },
                    StatType = StatType.Global_Banish,
                    Value = 1
                });
            }

            return nodes;
        }
    }
}