using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

// Keep TechTreeNode definition here as it's a data structure used by MetaUnlockPanel
[System.Serializable]
public class TechTreeNode
{
    public string id;
    [System.NonSerialized] public Vector2Int position;
    public List<string> dependencies;
    public bool unlocked;
}

namespace UI
{
    public class MetaUnlockPanel : MonoBehaviour
    {
        public TechTreeController techTreeController;

        [System.Serializable]
        public class TechTreeData
        {
            public List<TechTreeNode> nodes;
        }

        private List<TechTreeNode> _techTree;

        private void Awake()
        {
            if (techTreeController == null)
            {
                Debug.LogError("TechTreeController reference is NOT set in the Inspector on the MetaUnlockPanel!");
            }

            var path = Path.Combine(Application.streamingAssetsPath, "TechTree.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<TechTreeData>(json);
                _techTree = data.nodes;
            }
            else
            {
                Debug.LogError($"TechTree.json not found at path: {path}");
                _techTree = new List<TechTreeNode>();
            }
        }

        private void Start()
        {
            if (techTreeController != null && _techTree != null)
            {
                techTreeController.Initialize(_techTree);
                techTreeController.onNodeClicked += UnlockNode;
            }
        }

        public void UnlockNode(string nodeId)
        {
            Debug.Log($"MetaUnlockPanel received click for node: {nodeId}.");
            var nodeToUnlock = _techTree.Find(n => n.id == nodeId);

            if (nodeToUnlock != null && !nodeToUnlock.unlocked)
            {
                // Check if all dependencies are unlocked
                bool allDependenciesMet = nodeToUnlock.dependencies.All(depId =>
                {
                    var depNode = _techTree.Find(n => n.id == depId);
                    return depNode != null && depNode.unlocked;
                });

                if (allDependenciesMet)
                {
                    // Logic to spend resources would go here
                    Debug.Log($"Unlocking node: {nodeId}");
                    nodeToUnlock.unlocked = true;

                    // Re-initialize the controller to redraw the tree with the updated state
                    techTreeController.Initialize(_techTree);
                }
                else
                {
                    Debug.Log($"Cannot unlock node '{nodeId}': Dependencies not met.");
                }
            }
        }
    }
}