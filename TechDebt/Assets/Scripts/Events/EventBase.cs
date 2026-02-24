namespace Events
{
    public abstract class EventBase
    {
        protected int Probability  { get; set; } = 1;
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
        
        public virtual string GetDescription()
        {
            return $"{GetType().Name.Replace("Event", "")} - Prob: {GetProbability():F2}";
        }

        public virtual int GetProbability()
        {
            return Probability;
        }

    
    }
}