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
            if (map == null) { Debug.LogError("Map is null"); return; }
            if (map.LevelPool == null) { Debug.LogError("Map.LevelPool is null"); return; }
            
            Debug.Log($"Populating RoadMap nodes. Pool count: {map.LevelPool.Count}");

            foreach (MapLevel level in map.LevelPool)
            {
                var nodeView = new MapNodeView
                {
                    Node = level
                };
                _mapNodes.Add(nodeView);
                Debug.Log($"Added node: {level.DisplayName} (Id: {level.Id}, State: {level.CurrentState})");
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

            // Hide if MetaLocked
            if (mapLevel.CurrentState == MapNodeState.MetaLocked)
            {
                return false;
            }

            return true;
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
                        GameManager.Instance.Map.SetCurrentLevel(mapLevel);
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