using UnityEngine.Events;

namespace UI
{
    public class UIFocusablePanel: UIPanel
    {
        protected bool _isFocused = false;
        protected UnityAction onFocus;
        protected UnityAction onUnfocused;

        public void MarkFocused()
        {
            if (_isFocused)
            {
                return;
            }
            _isFocused = true;
            onFocus?.Invoke();
        }

        public void MarkUnfocused()
        {
            if (!_isFocused)
            {
                return;
            }

            _isFocused = false;
            onUnfocused?.Invoke();

        }
    }
}