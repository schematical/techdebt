
using System;
using UnityEngine;

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

    public static Rarity GetRandomRarity(float probibility = 0.01f)
    {
        Rarity resRarity = Rarity.Common;
        foreach (Rarity rarity in Enum.GetValues(typeof(Rarity)))
        {
            if (rarity == Rarity.Common)
            {
                continue;
            }

            if (UnityEngine.Random.value > probibility)
            {
                break;
            }
            resRarity = rarity;
        }

        return resRarity;
    }
}