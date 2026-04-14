using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UIProductRoadMap : UIMapPanel
    {
        public enum State
        {
            Display,
            Select
        };
        protected State CurrentState;
        

        public void Show(State _state = State.Display)
        {
            CurrentState = _state;
            GameManager.Instance.UIManager.ForcePause();
            connectorTilemap.color = Color.yellow;
            base.Show(); // This will call Refresh() -> PopulateNodes() and CenterTilemapOnCamera()
        }

        public override void PopulateNodes()
        {
            Map map = GameManager.Instance.Map;
            

            foreach (MapLevel level in map.LevelPool)
            {
                var nodeView = new MapNodeView
                {
                    Node = level
                };
                _mapNodes.Add(nodeView);
            }
        }

        protected override bool IsNodeVisible(MapNodeView nodeView)
        {
            if (nodeView.Node is not MapLevel mapLevel)
            {
                return false;
            }

            // Temporarily disable stakeholder gating as requested
            /*
            if (!string.IsNullOrEmpty(mapLevel.RequiredStakeholderId))
            {
                var stakeholder = GameManager.Instance.Stakeholders.FirstOrDefault(s => s.Id == mapLevel.RequiredStakeholderId);
                if (stakeholder == null || (stakeholder.CurrentState != MapNodeState.Unlocked && stakeholder.CurrentState != MapNodeState.Active))
                {
                    return false;
                }
            }
            */

            // Show EVERYTHING during layout testing
            return true;
        }
        public override void Close(bool forceClose = false)
        {
            if (panelState != UIState.Closed)
            {
                GameManager.Instance.UIManager.StopForcePause();
            }
            base.Close(forceClose);
        }
        public override void UpdateDetailsArea()
        {
            CleanUp();
            if (_selectedNode == null || _selectedNode.Node is not MapLevel mapLevel)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "Select a Sprint to view details.";
                return;
            }
        
            UIPanelLineSectionText header = AddLine<UIPanelLine>().Add<UIPanelLineSectionText>();
            header.h1(mapLevel.DisplayName);

            // AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = mapLevel.GetLevelDifficultyDesc();
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Sprint Duration: {mapLevel.SprintDuration} Days";

            if (mapLevel.LevelModifiers.Count > 0)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().h2("Sprint Modifiers:");
                foreach (MapLevelModifier modifier in mapLevel.LevelModifiers)
                {
                    AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = modifier.GetDescription(mapLevel);
                }
            }

            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().h2("Sprint Objectives:");
            foreach (MapLevelVictoryConditionBase condition in mapLevel.GetCombinedVictoryConditions())
            {
                // string prefix = condition.IsGlobal() ? "(Global) " : "";
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"{condition.GetDescription()}";
            }

            MetaProgressData metaData = MetaGameManager.LoadProgress();
            List<MapLevelReward> rewardsWithoutConditions = mapLevel.LevelRewards.FindAll(r => {
                if (r.VictoryConditions.Count > 0) return false;
                if (r.DependencyIds.Count > 0 && !r.DependencyIds.All(depId => metaData.claimedMetaRewardIds.Contains(depId))) return false;
                return true;
            });

            if (rewardsWithoutConditions.Count > 0)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().h2("Guaranteed Rewards:");
                foreach (MapLevelReward reward in rewardsWithoutConditions)
                {
                    AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = reward.Reward.GetTitle();
                }
            }

            List<MapLevelReward> bonusRewards = mapLevel.LevelRewards.FindAll(r => {
                if (r.VictoryConditions.Count == 0) return false;
                if (r.DependencyIds.Count > 0 && !r.DependencyIds.All(depId => metaData.claimedMetaRewardIds.Contains(depId))) return false;
                return true;
            });

            if (bonusRewards.Count > 0)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().h2("Bonus Objectives:");
                foreach (MapLevelReward reward in bonusRewards)
                {
                    bool isCompleted = reward.Type == MapLevelReward.MapLevelRewardType.Meta && metaData.claimedMetaRewardIds.Contains(reward.Id);
                    string status = isCompleted ? "<color=green>[COMPLETED]</color>" : "<color=red>[INCOMPLETE]</color>";
                    
                    AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"{status} {reward.Description}";
                    
                    foreach (MapLevelVictoryConditionBase condition in reward.VictoryConditions)
                    {
                        AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"  - Condition: {condition.GetDescription()}";
                    }
                }
            }

            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"\nState: {mapLevel.State}";
      
            if (CurrentState == State.Select && mapLevel.CurrentState == MapNodeState.Locked)
            {
                UIPanelButton startButton = AddLine<UIPanelButton>();
                startButton.text.text = "Start Sprint";
                startButton.button.onClick.RemoveAllListeners();
                startButton.button.onClick.AddListener(() =>
                {
                    GameManager.Instance.Map.SetCurrentLevel(mapLevel);
                    Close();
                    GameManager.Instance.GameLoopManager.BeginPlanPhase();
                });
            }
        }
    }
}