namespace DefaultNamespace.Util.Analytics
{
    public class RewardInteractionEvent: Unity.Services.Analytics.Event
    {
        public RewardInteractionEvent() : base("RewardInteractionEvent")
        {
        }

        public UI.UIMultiSelectOption.InteractionType InteractionType { set { SetParameter("InteractionType", value.ToString()); } }
        public string RewardId { set { SetParameter("RewardId", value); } }
    }
}