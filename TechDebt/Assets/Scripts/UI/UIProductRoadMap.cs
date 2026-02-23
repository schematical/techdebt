using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class UIProductRoadMap: UIPanel
    {
        public List<UIProductRoadMapLevelButton> Buttons = new List<UIProductRoadMapLevelButton>();
        public RectTransform RectTransform;
        public TextMeshProUGUI LevelDescriptionText;
        protected override void Start()
        {
            base.Start();
            
        }

        public void Show()
        {
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
            
            
            foreach (ProductRoadMapStage stage in Map.Stages)
            {
                int levelY = 0;
                foreach (ProductRoadMapLevel level in stage.Levels)
                {
                    UIProductRoadMapLevelButton button =
                        GameManager.Instance.prefabManager
                            .Create("UIProductRoadMapLevelButton", new Vector3(), transform)
                            .GetComponent<UIProductRoadMapLevelButton>();
                    button.Init(level, (name) => { LevelDescriptionText.text = name; });

                    float x = (width / Map.Stages.Count) * (stageX + 0.5f) - width / 2;
                    float y = (height / stage.Levels.Count) * (levelY + 0.5f) - height / 2;
                    button.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
                    
                    Buttons.Add(button);
                    levelY += 1;
                }

                stageX += 1;
            }
        }
        public void OnHover(PointerEventData eventData)
        {
            Debug.Log("OnHover");
        }
    }

  

  
}