
    public class RewardBase
    {
        public enum RewardType
        {
            Technology,
            StartingWorldObject,
            StartingStatValue,
            Modifier
        }
        public RewardType Type = RewardType.Technology;
        public string RewardId;
        public float RewardValue { get; set; } = -1;
    }
