using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
   
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

        protected virtual void SetNodeState(UIMetaUnlockMapNode node)
        {
            if (node.CurrentState != null) return;

            if (MetaGameManager.IsPrestigePointAllocationLeveledUp(node.ResourceType, node.Id))
            {
                node.CurrentState = MapNodeState.Unlocked;
                return;
            }

            if (node is UIMetaUnlockMapLeveledNode leveledNode && leveledNode.CurrentLevelIndex >= 0)
            {
                node.CurrentState = MapNodeState.Unlocked;
                return;
            }

            bool dependenciesMet = node.DependencyIds == null || node.DependencyIds.Count == 0 ||
                                   node.DependencyIds.All(depId => 
                                   {
                                       if (MetaGameManager.IsPrestigePointAllocationLeveledUp(node.ResourceType, depId)) return true;
                                       
                                       UIMetaUnlockMapNode depNode = GetNodeById(depId);
                                       return depNode != null && depNode.CurrentState == MapNodeState.Unlocked;
                                   });

            node.CurrentState = dependenciesMet ? MapNodeState.Locked : MapNodeState.MetaLocked;
        }

        protected virtual void UnallocateRecursive(MetaResourceType type, string id, int cost)
        {
            MetaProgressData progress = MetaGameManager.GetProgress();
            
            List<PrestigePointAllocation> dependents = progress.prestigePointAllocations.FindAll(r => {
                if (r.Type != type) return false;
                if (r.Type == MetaResourceType.Technology)
                {
                    Technology tech = MetaGameManager.GetAllTechnologies().Find(t => t.TechnologyID == r.Id);
                    return tech != null && tech.DependencyIds != null && tech.DependencyIds.Contains(id);
                }
                
                UIMetaUnlockMapNode node = GetNodeById(r.Id);
                return node != null && node.DependencyIds != null && node.DependencyIds.Contains(id);
            });

            foreach (PrestigePointAllocation dep in dependents)
            {
                int depCost = GetNodeCost(dep);
                UnallocateRecursive(dep.Type, dep.Id, depCost);
            }

            MetaGameManager.UpdatePrestigePointAllocation(type, id, cost);
        }

        public virtual UIMetaUnlockMapNode GetNodeById(string id)
        {
            UIMapPanel.MapNodeView nodeView = _panel.GetNodeById(id);
            if (nodeView == null)
            {
                return null;
            }
            return nodeView.Node as UIMetaUnlockMapNode;
        }
        
        protected virtual int GetNodeCost(PrestigePointAllocation resource)
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