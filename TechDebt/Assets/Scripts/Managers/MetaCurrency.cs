
public class MetaCurrency
{
    public static int CurrentAmount { get; private set; }

    public static void Add(int amount)
    {
        CurrentAmount += amount;
        SaveManager.SaveMetaCurrency(CurrentAmount);
    }

    public static void Spend(int amount)
    {
        CurrentAmount -= amount;
        SaveManager.SaveMetaCurrency(CurrentAmount);
    }

    public static void Load()
    {
        CurrentAmount = SaveManager.LoadMetaCurrency();
    }
}
