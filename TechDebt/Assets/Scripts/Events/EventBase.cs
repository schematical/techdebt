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
        
        public virtual void End()
        {
            if (!string.IsNullOrEmpty(EventEndText))
            {
                GameManager.Instance.UIManager.ShowAlert(EventEndText);
            }
            GameManager.Instance.EndEvent(this);
        }

        public virtual bool IsPossible()
        {
            return true;
        }
        public virtual bool IsOver()
        {
            return true;
        }
        
        public virtual string GetEventDescription()
        {
            return GetType().Name;
        }
    }
}