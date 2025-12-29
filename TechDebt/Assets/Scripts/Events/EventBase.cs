namespace Events
{
    public abstract class EventBase
    {
        public int Probility  { get; protected set; } = 1;
        public abstract void Apply();

        public virtual bool IsPossible()
        {
            return true;
        }
    }
}