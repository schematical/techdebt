using UnityEngine.UI;

namespace UI
{
    public class UIDeskMenuPanel: UIPanel
    {
        public Button researchButton;

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