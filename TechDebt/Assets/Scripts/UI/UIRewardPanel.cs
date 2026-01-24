using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIRewardPanel : MonoBehaviour
    {
        public Button openButton;
        public Image rewardImage;

        void Start()
        {
            openButton.onClick.AddListener(OnOpenClick);
        }
        public void Init()
        {
        
        }

        public void OnOpenClick()
        {
            Vector2 position = rewardImage.transform.position - new Vector3(0, 10);
            GameObject lazerGO =
                GameManager.Instance.prefabManager.Create("UILazarBeamPanel", position, transform);

            lazerGO.transform.SetAsFirstSibling();
            UILazerBeam lazerBeam = lazerGO.GetComponent<UILazerBeam>();
            lazerBeam.Init(20);

            lazerGO = GameManager.Instance.prefabManager.Create("UILazarBeamPanel", position, transform);
            lazerGO.transform.SetAsFirstSibling();
            
            lazerBeam = lazerGO.GetComponent<UILazerBeam>();
            lazerBeam.Init(-20);
           

            lazerGO = GameManager.Instance.prefabManager.Create("UILazarBeamPanel", position, transform);
            lazerGO.transform.SetAsFirstSibling();
            lazerBeam = lazerGO.GetComponent<UILazerBeam>();
            lazerBeam.Init(0);
            
        }
    }
}