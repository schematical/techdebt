using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class PrefabManager: MonoBehaviour
{
    public List<GameObject> Prefabs = new List<GameObject>();
    private Dictionary<string, List<GameObject>>Pool = new Dictionary<string, List<GameObject>>();
    
    public GameObject Create(string prefabId, Vector3 position)
    {
        // Ensure a pool for this packet type exists
        if (!Pool.ContainsKey(prefabId))
        {
            Pool[prefabId] = new List<GameObject>();
        }

        GameObject go = Pool[prefabId].FirstOrDefault(p => !p.gameObject.activeInHierarchy);

        if (go != null)
        {
            // Reactivate and re-initialize the pooled packet
            go.transform.position = position;
            go.SetActive(true);
        }
        else
        {
     
            GameObject prefab = GetPrefab(prefabId);
            if (prefab == null)
            {
                Debug.LogError($"Prefab {prefabId} not found");
            }
            else
            {
                go = Instantiate(prefab, position, Quaternion.identity);

                Pool[prefabId].Add(go); 
            }
        }
        
        return go;
    }

    public GameObject GetPrefab(string prefabId)
    {
        GameObject prefab = Prefabs.Find(p => p.name == prefabId);
        return prefab;
    }
}
