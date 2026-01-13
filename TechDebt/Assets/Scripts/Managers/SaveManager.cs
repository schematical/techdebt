
using UnityEngine;

public static class SaveManager
{
    public static void SaveMetaCurrency(int amount)
    {
        PlayerPrefs.SetInt("metaDollars", amount);
        PlayerPrefs.Save();
    }

    public static int LoadMetaCurrency()
    {
        return PlayerPrefs.GetInt("metaDollars", 0);
    }
}
