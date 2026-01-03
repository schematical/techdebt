using System;
using System.Collections.Generic;
using UnityEngine;
using Stats;

// Create a concrete class for serialization
[Serializable]
public class StatDataDictionary : SerializableDictionary<StatType, StatData> { }

[Serializable]
public class GameSaveData
{
    public List<InfrastructureSaveData> InfrastructureInstances = new List<InfrastructureSaveData>();
    public StatDataDictionary PlayerStats = new StatDataDictionary();
    // Add other global game states here, e.g., Day, UnlockedTech, etc.
}

[Serializable]
public class InfrastructureSaveData
{
    public string ID;
    public string Type;
    public Vector2Int GridPosition;
    public InfrastructureData.State CurrentState;
    public float CurrentLoad;
    public int CurrentSizeLevel;
    public StatDataDictionary Stats = new StatDataDictionary();
    public List<NetworkConnection> NetworkConnections;
}

// A serializable dictionary class to handle Unity's limitation with dictionaries.
[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<TKey> keys = new List<TKey>();

    [SerializeField]
    private List<TValue> values = new List<TValue>();

    // Save the dictionary to lists
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    // Load the dictionary from lists
    public void OnAfterDeserialize()
    {
        this.Clear();

        if (keys.Count != values.Count)
            throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

        for (int i = 0; i < keys.Count; i++)
            this.Add(keys[i], values[i]);
    }
}
