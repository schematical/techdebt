
    public class RewardBase
    {
        public enum RewardType
        {
            Technology_Unlocked,
            Technology_Locked,
            WorldObject_StartsOperational,
            WorldObjectType_StartsOperational,
            StartingStatValue,
            Modifier
        }
        public RewardType Type = RewardType.Technology_Locked;
        public string RewardId;
        public float RewardValue { get; set; } = -1;
    }
