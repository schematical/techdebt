using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class UIProductRoadMap: UIPanel
    {
        public enum State
        {
            Display,
            Select
        };
        protected State CurrentState;

        public List<UIProductRoadMapLevelButton> Buttons = new List<UIProductRoadMapLevelButton>();
        public RectTransform RectTransform;
        public TextMeshProUGUI LevelDescriptionText;
        private List<GameObject> Lines = new List<GameObject>();
        

        public void Show(State _state = State.Display)
        {
            CurrentState = _state;
            base.Show();
            ProductRoadMap Map = GameManager.Instance.ProductRoadMap;
            int stageX = 0;
            float width = RectTransform.rect.width;
            float height = RectTransform.rect.height;

            foreach (UIProductRoadMapLevelButton button in Buttons)
            {
                button.gameObject.SetActive(false);
            }
            Buttons.Clear();
            
            foreach (GameObject line in Lines)
            {
                Destroy(line);
            }
            Lines.Clear();

            List<List<UIProductRoadMapLevelButton>> stageButtons = new List<List<UIProductRoadMapLevelButton>>();

            foreach (ProductRoadMapStage stage in Map.Stages)
            {
                List<UIProductRoadMapLevelButton> currentStageButtons = new List<UIProductRoadMapLevelButton>();
                int levelY = 0;
                foreach (ProductRoadMapLevel level in stage.Levels)
                {
                    UIProductRoadMapLevelButton button =
                        GameManager.Instance.prefabManager
                            .Create("UIProductRoadMapLevelButton", new Vector3(), transform)
                            .GetComponent<UIProductRoadMapLevelButton>();

                    ButtonState state = ButtonState.Locked;
                    if (stageX < Map.CurrentStage)
                    {
                        if (stage.SelectedLevel == levelY)
                        {
                            state = ButtonState.Selected;
                        }
                        else
                        {
                            state = ButtonState.Passed;
                        }
                    }
                    else if (stageX == Map.CurrentStage)
                    {
                        state = ButtonState.Available;
                    }
                    
                    button.Init(level, levelY, state, (name) => { LevelDescriptionText.text = name; }, (lvl) =>
                    {
                        switch (CurrentState)
                        {
                            case State.Display:
                            break;
                            case State.Select:
                                stage.SetSelectedLevel(button.LevelIndex);
                                Close();
                                GameManager.Instance.GameLoopManager.BeginPlanPhase();
                            break;
                            default:
                                throw new NotImplementedException();
                        }
            
                    });

                    float x = (width / Map.Stages.Count) * (stageX + 0.5f) - width / 2;
                    float y = (height / stage.Levels.Count) * (levelY + 0.5f) - height / 2;
                    button.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
                    
                    Buttons.Add(button);
                    currentStageButtons.Add(button);
                    levelY += 1;
                }
                stageButtons.Add(currentStageButtons);
                stageX += 1;
            }

            // Draw lines connecting stages
            for (int i = 0; i < stageButtons.Count - 1; i++)
            {
                List<UIProductRoadMapLevelButton> currentStage = stageButtons[i];
                List<UIProductRoadMapLevelButton> nextStage = stageButtons[i + 1];

                foreach (UIProductRoadMapLevelButton startBtn in currentStage)
                {
                    foreach (UIProductRoadMapLevelButton endBtn in nextStage)
                    {
                        CreateLine(startBtn.GetComponent<RectTransform>().anchoredPosition, endBtn.GetComponent<RectTransform>().anchoredPosition);
                    }
                }
            }
        }

        private void CreateLine(Vector2 start, Vector2 end)
        {
            GameObject lineObj = new GameObject("Line", typeof(Image));
            lineObj.transform.SetParent(transform, false);
            lineObj.transform.SetAsFirstSibling(); 
            
            Image img = lineObj.GetComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 1.0f); 
            
            RectTransform rect = lineObj.GetComponent<RectTransform>();
            Vector2 dir = (end - start).normalized;
            float dist = Vector2.Distance(start, end);
            
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(dist, 2f); 
            rect.anchoredPosition = start + dir * dist * 0.5f;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rect.localEulerAngles = new Vector3(0, 0, angle);
            
            Lines.Add(lineObj);
        }
        
    }
}