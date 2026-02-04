
using System;
using System.Collections.Generic;
using DefaultNamespace;
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

    public static float GetDefaultScaleValue(Rarity rarity)
    {
        switch (rarity)
        {
            case (Rarity.Common):
                return 1.05f;
            case (Rarity.Uncommon):
                return 1.1f;
            case (Rarity.Rare):
                return 1.15f;
            case (Rarity.Epic):
                return 1.2f;
            default:
            case Rarity.Legendary:
                return 1.25f;
        }
    }
    public static Color GetColor(Rarity rarity)
    {
        switch (rarity)
        {
            case (Rarity.Common):
                return Color.white;
            case (Rarity.Uncommon):
                return Color.green;
            case (Rarity.Rare):
                return Color.blue;
            case (Rarity.Epic):
                return Color.purple;
            default:
            case Rarity.Legendary:
                return Color.yellow;;
        }
    }

    public static Sprite PaintIcon(Rarity rarity, Sprite sprite)
    {
        return GameManager.Instance.SpriteManager.ReplaceSpriteColors(
            sprite,
            new List<SimpleColorReplaceCombo>()
            {
                new SimpleColorReplaceCombo()
                {
                    findColor = SpriteManager.FromHex("#99e550"),
                    replaceColor = RarityHelper.GetColor(rarity)
                },
                new SimpleColorReplaceCombo()
                {
                    findColor = SpriteManager.FromHex("#75ae3e"),
                    replaceColor = SpriteManager.MakeDarker(RarityHelper.GetColor(rarity))
                },
                new SimpleColorReplaceCombo()
                {
                    findColor = SpriteManager.FromHex("#4b692f"),
                    replaceColor = SpriteManager.MakeDarker(RarityHelper.GetColor(rarity), 0.5f)
                },
            }
        );
    }
}