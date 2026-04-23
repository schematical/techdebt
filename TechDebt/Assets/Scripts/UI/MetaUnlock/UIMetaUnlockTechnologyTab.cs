using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI
{
    public class UIMetaUnlockTechnologyTab : UIMetaUnlockMapTabBase
    {
        public override string TabName => "Technology";

        public override void PopulateNodes(List<UIMapPanel.MapNodeView> mapNodes)
        {
            List<Technology> allTech = MetaGameManager.GetAllTechnologies();
            foreach (Technology tech in allTech)
            {
                bool isVisable = tech.UnlockConditions.All(condition =>
                {
                    if (condition.Type != UnlockCondition.ConditionType.Technology) return true;
                    return allTech.Any(t => t.TechnologyID == condition.TechnologyID);
                });

                if (isVisable)
                {
                    int prestigeCost = Mathf.CeilToInt(tech.ResearchTime / 30f);
                    UIMetaUnlockMapNode node = new UIMetaUnlockMapNode
                    {
                        ResourceType = MetaResourceType.Technology,
                        Id = tech.TechnologyID,
                        DisplayName = tech.DisplayName,
                        Description = tech.Description,
                        Direction = (MapNodeDirection)tech.Direction,
                        DependencyIds = tech.DependencyIds,
                        PrestigeCost = prestigeCost
                    };
                    SetNodeState(node);
                    mapNodes.Add(new UIMapPanel.MapNodeView { Node = node });
                }
            }
        }

        public override void UpdateDetailsArea()
        {
            _panel.CleanUp();
            
            MetaProgressData progress = MetaGameManager.GetProgress();
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
                    Technology depTech = MetaGameManager.GetAllTechnologies().Find(t => t.TechnologyID == depId);
                    if (depTech != null) depName = depTech.DisplayName;

                    bool met = MetaGameManager.IsResourceEquipped(mapNode.ResourceType, depId);
                    string status = met ? "<color=green>(MET)</color>" : "<color=red>(NOT MET)</color>";
                    _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $" - {depName} {status}";
                }
            }

            bool isEquipped = MetaGameManager.IsResourceEquipped(mapNode.ResourceType, mapNode.Id);

            if (isEquipped)
            {
                _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "\nSTATUS: ALLOCATED (START UNLOCKED)";
                
                List<PrestigePointAllocation> allocatedDependents = progress.prestigePointAllocations.FindAll(r => {
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

       
    }
}