using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PrefabManager: MonoBehaviour
{
    public List<GameObject> Prefabs = new List<GameObject>();
    private Dictionary<string, List<GameObject>>Pool = new Dictionary<string, List<GameObject>>();
   
}
