using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class PrefabManager: MonoBehaviour
{
    public List<GameObject> Prefabs = new List<GameObject>();
    private Dictionary<string, List<GameObject>>Pool = new Dictionary<string, List<GameObject>>();
    private List<string> particleIds;
    
    public GameObject Create(string prefabId, Vector3 position, Transform parentTransform = null)
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
            if (parentTransform != null)
            {
                go.transform.SetParent(parentTransform);
            }
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
                go = Instantiate(prefab, position, Quaternion.identity,  parentTransform);

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

    public GameObject CreateRandomParticle(Vector3 position, Transform parentTransform = null)
    {
        if (particleIds == null)
        {
            particleIds = new List<string>();
            foreach (GameObject go in Prefabs)
            {
                if (go.name.StartsWith("Particle_"))
                {
                    particleIds.Add(go.name);
                }
            }
        }

        int index = Random.Range(0, particleIds.Count);
        string goId = particleIds[index];
        return Create(goId, position, parentTransform);

    }
}
