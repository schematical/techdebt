namespace DefaultNamespace
{
    public class LocalizationData
    {
        public string Name;
        public string ToolTip;
    }
    public class LocalizationManager
    {
        public LocalizationData GetStatTypeData(StatType statType)
        {
            switch (statType)
            {
                case(StatType.Difficulty):
                    return new LocalizationData() {
                        Name = "Difficulty", 
                        ToolTip = "`Difficulty` affects things like how quickly traffic or tech debt increases."
                    };
                
                case(StatType.Global_LevelUpRarityModifier):
                    return new LocalizationData() {
                        Name = "Rarity", 
                        ToolTip = "`Rarity` Determines the rarity of level ups for your team and deployment rewards."
                    };
                default:
                    throw new System.NotImplementedException();
                
            }
        }
    }
}