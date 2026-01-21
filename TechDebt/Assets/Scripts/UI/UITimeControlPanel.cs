using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UITimeControlPanel : MonoBehaviour
    {
        private Color activeColor = new Color(0.5f, 0.8f, 1f); // Light blue for active button
        private Color inactiveColor = Color.gray;
        public Button pauseButton;
        public Button playButton;
        public Button fastForwardButton;
        public Button superFastForwardButton;

        void Start()
        {
            pauseButton.onClick.AddListener(() =>
            {
                GameManager.Instance.UIManager.SetTimeScalePause(true);
            });
            playButton.onClick.AddListener(() => GameManager.Instance.UIManager.SetTimeScalePlay(true));
            fastForwardButton.onClick.AddListener(() => GameManager.Instance.UIManager.SetTimeScaleFastForward(true));
            superFastForwardButton.onClick.AddListener(() => GameManager.Instance.UIManager.SetTimeScaleSuperFastForward(true));

            UpdateTimeScaleButtons();
        }
        


        public void UpdateTimeScaleButtons()
        {
            UIManager.TimeState _currentTimeState = GameManager.Instance.UIManager.GetCurrentTimeState();
            pauseButton.GetComponent<Image>().color =
                _currentTimeState == UIManager.TimeState.Paused ? activeColor : inactiveColor;
            playButton.GetComponent<Image>().color =
                _currentTimeState == UIManager.TimeState.Normal ? activeColor : inactiveColor;
            fastForwardButton.GetComponent<Image>().color =
                _currentTimeState == UIManager.TimeState.Fast ? activeColor : inactiveColor;
            superFastForwardButton.GetComponent<Image>().color =
                _currentTimeState == UIManager.TimeState.SuperFast ? activeColor : inactiveColor;
        }
    }
}