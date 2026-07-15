using System;
using System.Collections.Generic;
using System.Linq;
using MetaChallenges;
using UnityEngine;

namespace UI
{
    public class UIMetaUnlockTechnologyTab : UIMetaUnlockMapTabBase
    {
        public override string TabName => "Technology";

        public override void PopulateNodes(List<UIMapPanel.MapNodeView> mapNodes)
        {
            List<Technology> allTech = MetaGameManager.GetAllTechnologies();
            List<UIMetaUnlockMapNode> potentialNodes = new List<UIMetaUnlockMapNode>();

            // First pass: Find all nodes that meet non-technology requirements
            foreach (Technology tech in allTech)
            {
                bool isVisable = tech.UnlockConditions == null || tech.UnlockConditions.All(condition =>
                {
                    if (condition.Type != UnlockCondition.ConditionType.Technology) return true;
                    return true; // Tech dependencies handled in second pass
                });

                if (isVisable)
                {
                    int prestigeCost = Mathf.CeilToInt(tech.ResearchTime / 30f);
                    UIMetaUnlockMapNode node = new UIMetaUnlockMapNode
                    {
                        Id = tech.TechnologyID,
                        AllocationId = tech.TechnologyID,
                        Level = 1,
                        DisplayName = tech.DisplayName,
                        Description = tech.Description,
                        Direction = (MapNodeDirection)tech.Direction,
                        DependencyIds = tech.DependencyIds,
                        PrestigeCost = prestigeCost
                    };
                    SetNodeState(node);
                    potentialNodes.Add(node);
                }
            }

            // Second pass: Prune orphans
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int i = potentialNodes.Count - 1; i >= 0; i--)
                {
                    UIMetaUnlockMapNode node = potentialNodes[i];
                    if (node.DependencyIds == null || node.DependencyIds.Count == 0) continue;

                    bool hasMissingDependency = node.DependencyIds.Any(depId => !potentialNodes.Any(n => n.Id == depId));
                    if (hasMissingDependency)
                    {
                        Debug.Log($"[Meta Pruning] Removing orphan node: {node.Id} because a dependency is missing from the map.");
                        potentialNodes.RemoveAt(i);
                        changed = true;
                    }
                }
            }

            foreach (UIMetaUnlockMapNode node in potentialNodes)
            {
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
                    Technology depTech = MetaGameManager.GetAllTechnologies().Find(t => t.TechnologyID == depId);
                    if (depTech != null) depName = depTech.DisplayName;

                    bool met = MetaGameManager.IsPrestigePointAllocationLeveledUp(depId, 1);
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
                    Technology tech = MetaGameManager.GetAllTechnologies().Find(t => t.TechnologyID == r.Id);
                    return tech != null && tech.DependencyIds != null && tech.DependencyIds.Contains(mapNode.Id);
                });

                if (allocatedDependents.Count > 0)
                {
                    string depNames = string.Join(", ", allocatedDependents.Select(d => {
                        Technology t = MetaGameManager.GetAllTechnologies().Find(tech => tech.TechnologyID == d.Id);
                        return t != null ? t.DisplayName : d.Id;
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
                            MetaGameManager.UpdatePrestigePointAllocation(mapNode.AllocationId, mapNode.Level);
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

       
    }
}