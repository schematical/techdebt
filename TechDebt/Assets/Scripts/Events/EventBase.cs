namespace Events
{
    public abstract class EventBase
    {
        public int Probility  { get; protected set; } = 1;
        public string EventStartText { get; protected set; }
        public string EventEndText { get; protected set; }
        public virtual void Apply()
        {
            if (!string.IsNullOrEmpty(EventStartText))
            {
                GameManager.Instance.UIManager.ShowAlert(EventStartText);
            }
        }

        public virtual bool IsPossible()
        {
            return true;
        }
    }
}