using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIMetaUnlockLevelData
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public int PrestigeCost;
        public StatType StatType;
        public float Value;
    }

    public class UIMetaUnlockMapNode : iUIMapNode
    {
        public MetaResourceType ResourceType;
        public string Id { get; set; }

        public List<UIMetaUnlockLevelData> Levels;

        public int CurrentLevelIndex
        {
            get
            {
                if (Levels == null || Levels.Count == 0) return -1;
                for (int i = Levels.Count - 1; i >= 0; i--)
                {
                    if (MetaGameManager.IsResourceEquipped(ResourceType, Levels[i].Id))
                        return i;
                }
                return -1;
            }
        }

        private string _displayName;
        public string DisplayName 
        { 
            get 
            {
                if (Levels != null && Levels.Count > 0)
                {
                    if (Id == "OrgChart_CEO") return "CEO";
                    int idx = CurrentLevelIndex;
                    if (idx == Levels.Count - 1) return Levels[idx].DisplayName + " (Max)";
                    if (idx == -1) return Levels[0].DisplayName;
                    return Levels[idx].DisplayName + " -> " + Levels[idx + 1].DisplayName;
                }
                return _displayName;
            }
            set => _displayName = value;
        }

        private string _description;
        public string Description 
        { 
            get 
            {
                if (Levels != null && Levels.Count > 0)
                {
                    if (Id == "OrgChart_CEO") return "The visionary leader. Unlocked by default.";
                    int idx = CurrentLevelIndex;
                    if (idx == Levels.Count - 1) return $"Max level reached: {Levels[idx].DisplayName}.\n\n{Levels[idx].Description}";
                    
                    var next = Levels[idx + 1];
                    return $"Current: {(idx == -1 ? "None" : Levels[idx].DisplayName)}\nNext: {next.DisplayName}\n\n{next.Description}";
                }
                return _description;
            }
            set => _description = value;
        }

        public MapNodeDirection Direction { get; set; }
        public List<string> DependencyIds { get; set; }
        
        private int _prestigeCost;
        public int PrestigeCost 
        { 
            get 
            {
                if (Levels != null && Levels.Count > 0)
                {
                    int idx = CurrentLevelIndex;
                    if (idx == Levels.Count - 1) return 0;
                    return Levels[idx + 1].PrestigeCost;
                }
                return _prestigeCost;
            }
            set => _prestigeCost = value;
        }

        public StatType StatType;
        public float Value;

        public MapNodeState CurrentState
        {
            get
            {
                if (Levels != null && Levels.Count > 0)
                {
                    if (Id == "OrgChart_CEO") return MapNodeState.Unlocked;
                    if (CurrentLevelIndex == Levels.Count - 1) return MapNodeState.Unlocked;
                    
                    bool dependenciesMet = DependencyIds == null || DependencyIds.Count == 0 ||
                                           DependencyIds.All(depId => 
                                           {
                                               if (depId == "OrgChart_CEO") return true;
                                               return MetaGameManager.IsResourceEquipped(ResourceType, depId);
                                           });
                    return dependenciesMet ? MapNodeState.Locked : MapNodeState.MetaLocked;
                }

                if (MetaGameManager.IsResourceEquipped(ResourceType, Id))
                    return MapNodeState.Unlocked;

                bool dependenciesMet2 = DependencyIds == null || DependencyIds.Count == 0 ||
                                       DependencyIds.All(depId => MetaGameManager.IsResourceEquipped(ResourceType, depId));

                return dependenciesMet2 ? MapNodeState.Locked : MapNodeState.MetaLocked;
            }
        }

        public float GetProgress() => 0;

        public UnityEngine.Tilemaps.TileBase GetTile()
        {
            string tileId = "TechTreeLockedTile";
            switch (CurrentState)
            {
                case MapNodeState.MetaLocked:
                    tileId = "TechTreeLockedTile";
                    break;
                case MapNodeState.Locked:
                    tileId = "TechTreeUnlockedTile";
                    break;
                case MapNodeState.Active:
                    tileId = "TechTreeResearching";
                    break;
                case MapNodeState.Unlocked:
                    tileId = "TechTreeResearched";
                    break;
            }
            return GameManager.Instance.prefabManager.GetTile(tileId);
        }

        public void OnSelected(UIMapPanel panel)
        {
        }
    }

    public abstract class UIMetaUnlockMapTabBase
    {
        protected UIMapPanel _panel;
        public abstract string TabName { get; }

        public virtual void Initialize(UIMapPanel panel)
        {
            _panel = panel;
        }

        public abstract void PopulateNodes(List<UIMapPanel.MapNodeView> mapNodes);
        public abstract void UpdateDetailsArea();

        protected virtual void UnallocateRecursive(MetaResourceType type, string id, int cost)
        {
            MetaProgressData progress = MetaGameManager.LoadProgress();
            
            List<MetaUnlockResource> dependents = progress.prestigePointAllocations.FindAll(r => {
                if (r.Type != type) return false;
                if (r.Type == MetaResourceType.Technology)
                {
                    Technology tech = MetaGameManager.GetAllTechnologies().Find(t => t.TechnologyID == r.Id);
                    return tech != null && tech.DependencyIds != null && tech.DependencyIds.Contains(id);
                }
                
                UIMetaUnlockMapNode node = GetNodeById(r.Id);
                return node != null && node.DependencyIds != null && node.DependencyIds.Contains(id);
            });

            foreach (MetaUnlockResource dep in dependents)
            {
                int depCost = GetNodeCost(dep);
                UnallocateRecursive(dep.Type, dep.Id, depCost);
            }

            MetaGameManager.ToggleResourceEquip(type, id, cost);
        }

        protected abstract UIMetaUnlockMapNode GetNodeById(string id);
        
        protected virtual int GetNodeCost(MetaUnlockResource resource)
        {
            if (resource.Type == MetaResourceType.Technology)
            {
                Technology depTech = MetaGameManager.GetAllTechnologies().Find(t => t.TechnologyID == resource.Id);
                return depTech != null ? Mathf.CeilToInt(depTech.ResearchTime / 30f) : 0;
            }
            
            UIMetaUnlockMapNode node = GetNodeById(resource.Id);
            return node != null ? node.PrestigeCost : 0;
        }
    }
}