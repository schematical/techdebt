using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// The TechTreeNode is now primarily a VIEW model for the TechTreeController.
// The main data model is the Technology class from MetaGameManager.
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
        
        // This now holds the authoritative list of technologies from the game manager.
        private List<Technology> _technologies;

        private void Awake()
        {
            if (techTreeController == null)
            {
                Debug.LogError("TechTreeController reference is NOT set in the Inspector on the MetaUnlockPanel!");
            }

            // Get the technology data from the central manager.
            _technologies = MetaGameManager.GetAllTechnologies();

            if (_technologies == null || _technologies.Count == 0)
            {
                Debug.LogError("No technologies loaded from MetaGameManager.");
            }
        }

        private void Start()
        {
            closeButton.onClick.AddListener(OnClose);
            DrawTree();
        }

        private void DrawTree()
        {
            if (techTreeController != null && _technologies != null)
            {
                var savedProgress = MetaGameManager.ProgressData;
                
                // Convert Technology list to TechTreeNode list for the controller
                var techTreeNodes = _technologies.Select(tech => new TechTreeNode
                {
                    id = tech.TechnologyID,
                    DisplayName = tech.DisplayName,
                    Description = tech.Description,
                    ResearchPointCost = tech.ResearchPointCost,
                    dependencies = tech.RequiredTechnologies,
                    unlocked = savedProgress.unlockedNodeIds.Contains(tech.TechnologyID)
                }).ToList();
                
                techTreeController.Initialize(techTreeNodes);
                techTreeController.onNodeClicked -= UnlockNode; // Ensure we don't double-subscribe
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
            var nodeToUnlock = _technologies.Find(n => n.TechnologyID == nodeId);
            var progress = MetaGameManager.ProgressData;

            if (nodeToUnlock != null && !progress.unlockedNodeIds.Contains(nodeId))
            {
                // Check if all dependencies are unlocked
                bool allDependenciesMet = nodeToUnlock.RequiredTechnologies.All(depId =>
                    progress.unlockedNodeIds.Contains(depId));

                if (allDependenciesMet)
                {
                    // TODO: Check if the player has enough research points
                    // if (progress.researchPoints < nodeToUnlock.ResearchPointCost)
                    // {
                    //     Debug.Log($"Cannot unlock node '{nodeId}': Not enough research points.");
                    //     return;
                    // }
                    
                    Debug.Log($"Unlocking node: {nodeId}");
                    
                    // Update and save the progress via MetaGameManager
                    // TODO: Subtract research points
                    // progress.researchPoints -= nodeToUnlock.ResearchPointCost;
                    if (!progress.unlockedNodeIds.Contains(nodeId))
                    {
                        progress.unlockedNodeIds.Add(nodeId);
                    }
                    MetaGameManager.SaveProgress(progress);

                    // Redraw the tree with the updated state
                    DrawTree();
                }
                else
                {
                    Debug.Log($"Cannot unlock node '{nodeId}': Dependencies not met.");
                }
            }
        }
    }
}