using Stats;
using UnityEngine;

namespace Events
{
    public class LeakedSecretEvent : EventBase
    {
        public int Version { get; private set; } = 1;
        public LeakedSecretEvent()
        {
            EventStartText = "Some how we are sending hundreds on unauthorized email.";
            EventEndText = "TODO: Make a cycle credentials task you can trigger."; // Or perhaps an actual result
            Probility = 1;
        }
        
      
    }
}
