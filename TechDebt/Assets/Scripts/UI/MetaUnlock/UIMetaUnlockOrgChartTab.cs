using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UIMetaUnlockOrgChartTab : UIMetaUnlockMapTabBase
    {
        public override string TabName => "Org Chart";

        public override void PopulateNodes(List<UIMapPanel.MapNodeView> mapNodes)
        {
            foreach (Stakeholder node in GetOrgChartDefinitions())
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
                _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "Select a position on the Org Chart to view details.";
                return;
            }

            UIMetaUnlockMapLeveledNode mapLeveledNode = (UIMetaUnlockMapLeveledNode)selectedNode.Node;
            
            UIPanelLineSectionText header = _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>();
            header.h1(mapLeveledNode.DisplayName);
            
            _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = mapLeveledNode.Description;
            
            int currentLevelIdx = mapLeveledNode.CurrentLevelIndex;
            bool isMaxLevel = mapLeveledNode.Levels != null && currentLevelIdx == mapLeveledNode.Levels.Count - 1;
            
            int nextLevelIdx = currentLevelIdx + 1;
            int nextLevelCost = 0;
            UIMetaUnlockLevelData nextLevel = null;
            if (mapLeveledNode.Levels != null && nextLevelIdx >= 0 && nextLevelIdx < mapLeveledNode.Levels.Count)
            {
                nextLevel = mapLeveledNode.Levels[nextLevelIdx];
                nextLevelCost = nextLevel.PrestigeCost;
            }

            bool conditionsMet = true;
            if (nextLevel != null && nextLevel.UnlockConditions != null)
            {
                foreach (UnlockCondition condition in nextLevel.UnlockConditions)
                {
                    if (!condition.IsUnlocked())
                    {
                        conditionsMet = false;
                        break;
                    }
                }
            }

            if (nextLevelCost > 0 && !isMaxLevel && conditionsMet)
            {
                _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"\nCost to Upgrade: {nextLevelCost} Vested Shares";
            }

            bool canAfford = GetAvailablePrestigePoints() >= nextLevelCost;
            bool readyToUnlock = mapLeveledNode.CurrentState != MapNodeState.MetaLocked;

            int i = 0;
            if (mapLeveledNode.Levels != null)
            {
                foreach (UIMetaUnlockLevelData level in mapLeveledNode.Levels)
                {
                    UIPanelLine levelLine = _panel.AddLine<UIPanelLine>();
                    Color color = Color.grey;
                
                    if (currentLevelIdx == i)
                    {
                        color = Color.white;
                    }
                    else if (currentLevelIdx < i)
                    {
                        color = Color.darkGray;
                    }
                    
                    UIPanelLineSectionText titleText = levelLine.Add<UIPanelLineSectionText>();
                    
                    bool levelConditionsMet = true;
                    if (level.UnlockConditions != null)
                    {
                        foreach (UnlockCondition condition in level.UnlockConditions)
                        {
                            if (!condition.IsUnlocked())
                            {
                                levelConditionsMet = false;
                                break;
                            }
                        }
                    }

                    string levelTitle = $"Level {i + 1}: {level.DisplayName}";
                    if (!levelConditionsMet)
                    {
                        levelTitle += " (Locked)";
                    }
                    
                    titleText.h2(levelTitle);
                    titleText.text.color = color;

                    UIPanelLineSectionText descText = _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>();
                    descText.text.text = $"{level.Description}";
                    descText.text.color = color;
                    
                    i++;
                }
            }
            
            // Action Buttons
            if (!isMaxLevel && conditionsMet)
            {
                if (readyToUnlock)
                {
                    if (canAfford)
                    {
                        string hireText = currentLevelIdx >= 0 ? "Promote" : "Hire";
                        _panel.AddButton(hireText, () =>
                        {
                            MetaGameManager.UpdatePrestigePointAllocation(mapLeveledNode.AllocationId, currentLevelIdx + 2);
                            _panel.Refresh();
                        });
                    }
                    else
                    {
                        _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "<color=red>NOT ENOUGH VESTED SHARES</color>";
                    }
                }
                else
                {
                    _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "<color=red>PREREQUISITES NOT MET</color>";
                }
            }

            if (currentLevelIdx > -1)
            {
                string demoteText = currentLevelIdx > 0 ? "Demote" : "Remove";
                _panel.AddButton(demoteText, () =>
                {
                    UIMetaUnlockLevelData currentLevel = mapLeveledNode.Levels[currentLevelIdx];
                    UIMetaUnlockMapNode tempNode = new UIMetaUnlockMapNode 
                    { 
                        Id = mapLeveledNode.Id, 
                        AllocationId = mapLeveledNode.AllocationId, 
                        Level = currentLevelIdx + 1,
                        PrestigeCost = currentLevel.PrestigeCost
                    };
                    UnallocateRecursive(tempNode);
                    _panel.Refresh();
                });
            }
        }

        private List<Stakeholder> GetOrgChartDefinitions()
        {
            return GameManager.Instance.Stakeholders;
        }
    }


}