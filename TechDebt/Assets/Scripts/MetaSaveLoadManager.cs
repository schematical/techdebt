using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class MetaSaveLoadManager
{
    private static MetaProgressData _progressData;
    private static string _savePath;

    public static MetaProgressData ProgressData
    {
        get
        {
            if (_progressData == null)
            {
                LoadProgress();
            }
            return _progressData;
        }
    }

    static MetaSaveLoadManager()
    {
        // Static constructor called once when the class is first accessed
        _savePath = Path.Combine(Application.persistentDataPath, "meta_progress.json");
        LoadProgress();
    }

    public static void SaveProgress()
    {
        string json = JsonUtility.ToJson(_progressData, true);
        File.WriteAllText(_savePath, json);
        Debug.Log($"Progress saved to {_savePath}");
    }

    public static void LoadProgress()
    {
        if (File.Exists(_savePath))
        {
            string json = File.ReadAllText(_savePath);
            _progressData = JsonUtility.FromJson<MetaProgressData>(json);
            Debug.Log($"Progress loaded from {_savePath}");
        }
        else
        {
            _progressData = new MetaProgressData();
            Debug.Log("No save file found. Created new progress data.");
        }
    }

    // Optional: Reset progress for testing or new game
    public static void ResetProgress()
    {
        _progressData = new MetaProgressData();
        SaveProgress();
        Debug.Log("Meta progress reset.");
    }
}
