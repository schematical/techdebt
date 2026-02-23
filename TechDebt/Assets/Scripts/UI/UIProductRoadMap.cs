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
            int stageY = 0;
            foreach (ProductRoadMapStage stage in Map.Stages)
            {
                int levelX = 0;
                foreach (ProductRoadMapLevel level in stage.Levels)
                {
                    UIProductRoadMapLevelButton button =
                        GameManager.Instance.prefabManager
                            .Create("UIProductRoadMapLevelButton", new Vector3(), transform)
                            .GetComponent<UIProductRoadMapLevelButton>();
                    levelX += 1;
                }

                stageY += 1;
            }
        }
    }

  
}