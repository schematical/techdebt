namespace DefaultNamespace.Util.Analytics
{
    public class DaySummaryEvent: Unity.Services.Analytics.Event
    {
        public DaySummaryEvent() : base("DaySummaryEvent")
        {
        }

        public string SprintLevel { set { SetParameter("SprintLevel", value); } }
        public int Day { set { SetParameter("Day", value); } }
        public int SprintNumber { set { SetParameter("SprintNumber", value); } }
    }
}