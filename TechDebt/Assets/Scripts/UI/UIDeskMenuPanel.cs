using UnityEngine.UI;

namespace UI
{
    public class UIDeskMenuPanel: UIPanel
    {
        public Button researchButton;
        public Button startReleaseButton;

        void Start()
        {
            researchButton.onClick.AddListener(OnResearchClicked);
        }

        private void OnResearchClicked()
        {
            GameManager.Instance.UIManager.ToggleTechTreePanel();
            gameObject.SetActive(false);
        }
    }
}