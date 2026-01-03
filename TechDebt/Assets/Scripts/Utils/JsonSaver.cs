using System.IO;
using UnityEngine;

public static class JsonSaver
{
    public static void SaveToFile(object data, string path)
    {
        string json = JsonUtility.ToJson(data, true); // 'true' for pretty print
        File.WriteAllText(path, json);
    }

    public static T LoadFromFile<T>(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"Save file not found at {path}");
            return default;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<T>(json);
    }
}
