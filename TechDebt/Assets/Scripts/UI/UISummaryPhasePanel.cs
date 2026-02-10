using UnityEngine.UI;

namespace UI
{
    public class UISummaryPhasePanel: UIPanel
    {
        public UITextArea textArea;
        public Button continueButton;

        void Start()
        {
            continueButton.onClick.AddListener(OnContinue);
        }

        public override void Show()
        {
            base.Show();
        }

        public override void Close(bool forceClose = false)
        {
            base.Close(forceClose);
        }

        private void OnContinue()
        {
            GameManager.Instance.GameLoopManager.BeginPlanPhase();
            Close();
        }
    }
}