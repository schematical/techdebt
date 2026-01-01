using Stats;
using UnityEngine;

namespace Events
{
    public class NothingEvent : EventBase
    {
        public int Version { get; private set; } = 1;
        public NothingEvent()
        {
            EventStartText = "Nothing Exciting Today";
            EventEndText = "Calm seas"; // Or perhaps an actual result
            Probility = 1;
        }
        
      
    }
}
