using UnityEngine;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    private const string SaveFileName = "savegame.json";

    public static string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    public void SaveGame(GameSaveData data)
    {
        string path = GetSavePath();
        Debug.Log($"Saving game to {path}");
        JsonSaver.SaveToFile(data, path);
    }

    public GameSaveData LoadGame()
    {
        string path = GetSavePath();
        Debug.Log($"Loading game from {path}");
        return JsonSaver.LoadFromFile<GameSaveData>(path);
    }
}
