namespace DefaultNamespace.Util.Analytics
{
    public class PrestigePointAllocationEvent: Unity.Services.Analytics.Event
    {
        public PrestigePointAllocationEvent() : base("PrestigePointAllocationEvent")
        {
        }

        public string AllocationId { set { SetParameter("RewardId", value); } }
        public int Level { set { SetParameter("RewardId", value); } }
    }
}