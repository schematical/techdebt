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

        public override void Show()
        {
            base.Show();
            int daysLeftInSprint = GameManager.Instance.GameLoopManager.GetDaysLeftInSprint();
            if (daysLeftInSprint > 0)
            {
                titleText.text = $"{daysLeftInSprint + 1} Days Left In This Sprint";
            }
            else
            {
                titleText.text = $"Launch Day!";
            }

          
        }
        public void OnStartDayClicked()
        {
            GameManager.Instance.GameLoopManager.BeginPlayPhase();
            Close();
        }
    }
}