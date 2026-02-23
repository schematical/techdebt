using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class UIProductRoadMap: UIPanel
    {
        public void Show()
        {
            base.Show();
            ProductRoadMap Map = GameManager.Instance.ProductRoadMap;
            int stageX = 0;
            RectTransform rect = GetComponent<RectTransform>();
            float width = rect.rect.width;
            float height = rect.rect.height;

            foreach (ProductRoadMapStage stage in Map.Stages)
            {
                int levelY = 0;
                foreach (ProductRoadMapLevel level in stage.Levels)
                {
                    UIProductRoadMapLevelButton button =
                        GameManager.Instance.prefabManager
                            .Create("UIProductRoadMapLevelButton", new Vector3(), transform)
                            .GetComponent<UIProductRoadMapLevelButton>();
                    button.Init(level);

                    float x = (width / Map.Stages.Count) * (stageX + 0.5f) - width / 2;
                    float y = (height / stage.Levels.Count) * (levelY + 0.5f) - height / 2;
                    button.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);

                    levelY += 1;
                }

                stageX += 1;
            }
        }
    }

  
}