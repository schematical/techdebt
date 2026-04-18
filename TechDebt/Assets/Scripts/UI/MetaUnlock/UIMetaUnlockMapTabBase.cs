using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIMetaUnlockMapNode : iUIMapNode
    {
        public MetaResourceType ResourceType;
        public string Id { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual string Description { get; set; }
        public MapNodeDirection Direction { get; set; }
        public List<string> DependencyIds { get; set; }
        public virtual int PrestigeCost { get; set; }
        public StatType StatType;
        public float Value;
        public bool unlockedByDefault = false;

        public virtual MapNodeState CurrentState
        {
            get
            {
                if (unlockedByDefault)
                {
                    return MapNodeState.Active;
                }
                if (MetaGameManager.IsResourceEquipped(ResourceType, Id))
                    return MapNodeState.Unlocked;

                bool dependenciesMet = DependencyIds == null || DependencyIds.Count == 0 ||
                                       DependencyIds.All(depId => MetaGameManager.IsResourceEquipped(ResourceType, depId));

                return dependenciesMet ? MapNodeState.Locked : MapNodeState.MetaLocked;
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