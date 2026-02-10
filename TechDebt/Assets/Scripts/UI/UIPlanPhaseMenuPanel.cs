using UnityEngine.UI;

namespace UI
{
    public class UIPlanPhaseMenuPanel:  UIPanel
    {
        public Button startDayButton;
        protected override void Start()
        {
            base.Start();
            Shake(5);
            startDayButton.onClick.AddListener(OnStartDayClicked);
        }

        public void OnStartDayClicked()
        {
            GameManager.Instance.GameLoopManager.BeginPlayPhase();
            Close();
        }
    }
}