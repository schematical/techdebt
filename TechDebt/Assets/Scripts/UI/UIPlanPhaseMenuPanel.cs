using UnityEngine.UI;

namespace UI
{
    public class UIPlanPhaseMenuPanel:  UIPanel
    {
        public Button startDayButton;
        protected override void Start()
        {
            base.Start();
            startDayButton.onClick.AddListener(OnStartDayClicked);
        }

        public void OnStartDayClicked()
        {
            GameManager.Instance.GameLoopManager.BeginPlayPhase();
            Close();
        }
    }
}