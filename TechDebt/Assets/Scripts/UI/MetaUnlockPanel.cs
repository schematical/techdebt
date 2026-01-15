using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Keep TechTreeNode definition here as it's a data structure used by MetaUnlockPanel
[System.Serializable]
public class TechTreeNode
{
    public string id;
    public string DisplayName;
    public string Description;
    public int ResearchPointCost;
    [System.NonSerialized] public Vector2Int position;
    public List<string> dependencies;
    public bool unlocked;
}

namespace UI
{
    public class MetaUnlockPanel : MonoBehaviour
    {
        public TechTreeController techTreeController;
        public Button closeButton;

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
            closeButton.onClick.AddListener(OnClose);
            if (techTreeController != null && _techTree != null)
            {
                // Apply loaded progress to the tech tree data using MetaSaveLoadManager
                var savedProgress = MetaGameManager.ProgressData;
                foreach (var node in _techTree)
                {
                    node.unlocked = savedProgress.unlockedNodeIds.Contains(node.id);
                }
                
                techTreeController.Initialize(_techTree);
                techTreeController.onNodeClicked += UnlockNode;
            }
        }

        public void OnClose()
        {
            UIMainMenuCanvas.Instance.ClosePanels();
            UIMainMenuCanvas.Instance.mainMenu.gameObject.SetActive(true);
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
                    // Use MetaSaveLoadManager to check dependency unlock status
                    return depNode != null && MetaGameManager.ProgressData.unlockedNodeIds.Contains(depId);
                });

                if (allDependenciesMet)
                {
                    // TODO: Check if the player has enough research points
                    // if (MetaSaveLoadManager.ProgressData.researchPoints < nodeToUnlock.ResearchPointCost)
                    // {
                    //     Debug.Log($"Cannot unlock node '{nodeId}': Not enough research points.");
                    //     return;
                    // }
                    
                    Debug.Log($"Unlocking node: {nodeId}");
                    nodeToUnlock.unlocked = true;

                    // Update and save the progress via MetaSaveLoadManager
                    var progress = MetaGameManager.ProgressData;
                    // TODO: Subtract research points
                    // progress.researchPoints -= nodeToUnlock.ResearchPointCost;
                    if (!progress.unlockedNodeIds.Contains(nodeId))
                    {
                        progress.unlockedNodeIds.Add(nodeId);
                    }
                    MetaGameManager.SaveProgress();

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