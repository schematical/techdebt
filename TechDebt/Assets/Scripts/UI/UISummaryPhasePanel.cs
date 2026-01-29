using UnityEngine.UI;

namespace UI
{
    public class UISummaryPhasePanel: UIPanel
    {
        public UITextArea textArea;
        public Button continueButton;

        void Start()
        {continueButton.onClick.AddListener(OnContinue);
        }

        private void OnContinue()
        {
            GameManager.Instance.GameLoopManager.BeginPlanPhase();
            gameObject.SetActive(false);
        }
    }
}