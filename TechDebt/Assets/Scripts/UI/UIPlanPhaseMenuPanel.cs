using UnityEngine.UI;

namespace UI
{
    public class UIPlanPhaseMenuPanel:  UIPanel
    {
        public Button startDayButton;
        void Start()
        {
       
            startDayButton.onClick.AddListener(OnStartDayClicked);
        }

        public void OnStartDayClicked()
        {
            GameManager.Instance.GameLoopManager.BeginPlayPhase();
            gameObject.SetActive(false);
        }
    }
}