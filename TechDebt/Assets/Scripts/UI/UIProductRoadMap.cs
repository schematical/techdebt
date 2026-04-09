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

        [Header("RoadMap Details Area")]
        public TextMeshProUGUI LevelDescriptionText;
        public UIButton startSprintButton;

        public void Show(State _state = State.Display)
        {
            CurrentState = _state;
            base.Show(); // This will call Refresh() -> PopulateNodes() and CenterTilemapOnCamera()
        }

        public override void PopulateNodes()
        {
            Map map = GameManager.Instance.Map;
            if (map == null || map.Stages == null) return;

            MapLevel previousSelectedLevel = null;

            for (int stageIndex = 0; stageIndex < map.Stages.Count; stageIndex++)
            {
                MapStage stage = map.Stages[stageIndex];

                for (int levelIndex = 0; levelIndex < stage.Levels.Count; levelIndex++)
                {
                    MapLevel level = stage.Levels[levelIndex];

                    // Map DependencyIds for procedural layout
                    level.DependencyIds.Clear();
                    if (stageIndex > 0)
                    {
                        // In a roguelite map, usually you depend on the previously selected node
                        if (previousSelectedLevel != null)
                        {
                            level.DependencyIds.Add(previousSelectedLevel.Id);
                        }
                        else
                        {
                            // If no specific level was selected yet, depend on all from previous stage?
                            // For procedural generation to work right, we need at least one root.
                            // We will link it to the first level of the previous stage if nothing else is available.
                            level.DependencyIds.Add(map.Stages[stageIndex - 1].Levels[0].Id);
                        }
                    }

                    var nodeView = new MapNodeView
                    {
                        Node = level
                    };
                    _mapNodes.Add(nodeView);
                }

                if (stageIndex < map.CurrentStageIndex && stage.SelectedLevel != -1)
                {
                    previousSelectedLevel = stage.Levels[stage.SelectedLevel];
                }
                else
                {
                    previousSelectedLevel = null;
                }
            }
        }

        protected override bool IsNodeVisible(MapNodeView nodeView)
        {
            if (nodeView.Node is not MapLevel mapLevel) return false;

            // Optional Stakeholder gating
            if (!string.IsNullOrEmpty(mapLevel.RequiredStakeholderId))
            {
                var stakeholder = GameManager.Instance.Stakeholders.FirstOrDefault(s => s.Id == mapLevel.RequiredStakeholderId);
                if (stakeholder == null || stakeholder.State == MapNodeState.MetaLocked || stakeholder.State == MapNodeState.Locked)
                {
                    // Stakeholder not unlocked, node remains hidden
                    return false;
                }
            }

            return base.IsNodeVisible(nodeView);
        }

        public override void UpdateDetailsArea()
        {
            if (_selectedNode == null || _selectedNode.Node is not MapLevel mapLevel)
            {
                if (LevelDescriptionText != null) LevelDescriptionText.text = "Select a Sprint to view details.";
                if (startSprintButton != null) startSprintButton.gameObject.SetActive(false);
                return;
            }

            if (LevelDescriptionText != null)
            {
                LevelDescriptionText.text = mapLevel.GetDescription();
            }

            if (startSprintButton != null)
            {
                if (CurrentState == State.Select && mapLevel.CurrentState == MapNodeState.Locked)
                {
                    startSprintButton.gameObject.SetActive(true);
                    startSprintButton.buttonText.text = "Start Sprint";
                    startSprintButton.button.onClick.RemoveAllListeners();
                    startSprintButton.button.onClick.AddListener(() =>
                    {
                        mapLevel.GetStage().SetLevel(mapLevel);
                        Close();
                        GameManager.Instance.GameLoopManager.BeginPlanPhase();
                    });
                }
                else
                {
                    startSprintButton.gameObject.SetActive(false);
                }
            }
        }
    }
}