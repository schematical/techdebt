using System;

namespace DefaultNamespace
{
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
    }

    public class RarityHelper
    {
        public static Rarity GetNextRarity(Rarity rarity)
        {
            switch (rarity)
            {

                case (Rarity.Common):
                    return Rarity.Uncommon;
                case (Rarity.Uncommon):
                    return Rarity.Rare;
                case (Rarity.Rare):
                    return Rarity.Epic;
                case (Rarity.Epic):
                    return Rarity.Legendary;
                default:
                case Rarity.Legendary:
                    throw new SystemException("This shouldn't hit. We are at max legendary.");
                    
            }
        }
    }
}