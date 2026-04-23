using System;
using System.Collections.Generic;
using System.Linq;
using MetaChallenges;
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

            if (MetaGameManager.IsPrestigePointAllocationLeveledUp(node.AllocationId, node.Level))
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
                                       UIMetaUnlockMapNode depNode = GetNodeById(depId);
                                       if (depNode != null && MetaGameManager.IsPrestigePointAllocationLeveledUp(depNode.AllocationId, depNode.Level)) return true;
                                       return depNode != null && depNode.CurrentState == MapNodeState.Unlocked;
                                   });

            node.CurrentState = dependenciesMet ? MapNodeState.Locked : MapNodeState.MetaLocked;
        }

        protected virtual void UnallocateRecursive(UIMetaUnlockMapNode nodeToUnallocate)
        {
            MetaProgressData progress = MetaGameManager.GetProgress();
            
            // 1. Find and refund Technology dependents
            List<Technology> allTech = MetaGameManager.GetAllTechnologies();
            foreach (Technology tech in allTech)
            {
                if (tech.DependencyIds != null && tech.DependencyIds.Contains(nodeToUnallocate.Id))
                {
                    if (MetaGameManager.IsPrestigePointAllocationLeveledUp(tech.TechnologyID, 1))
                    {
                        UIMetaUnlockMapNode techNode = new UIMetaUnlockMapNode { Id = tech.TechnologyID, AllocationId = tech.TechnologyID, Level = 1 };
                        UnallocateRecursive(techNode);
                    }
                }
            }

            // 2. Find and refund dependents in the map (e.g., Bonuses chain or Org Chart)
            foreach (UIMapPanel.MapNodeView nodeView in _panel.GetAllNodes())
            {
                UIMetaUnlockMapNode otherNode = nodeView.Node as UIMetaUnlockMapNode;
                if (otherNode != null && otherNode.Id != nodeToUnallocate.Id && otherNode.DependencyIds != null && otherNode.DependencyIds.Contains(nodeToUnallocate.Id))
                {
                    if (MetaGameManager.IsPrestigePointAllocationLeveledUp(otherNode.AllocationId, otherNode.Level))
                    {
                        UnallocateRecursive(otherNode);
                    }
                }
            }
            
            // 3. Perform the actual unallocation for this node
            MetaPrestigePointAllocatable allocatable = MetaGameManager.GetPrestigePointAllocatables().Find(a => a.Id == nodeToUnallocate.AllocationId);
            int cost = 0;
            if (allocatable != null && nodeToUnallocate.Level > 0 && nodeToUnallocate.Level <= allocatable.levels.Count)
            {
                cost = allocatable.levels[nodeToUnallocate.Level - 1].cost;
            }

            MetaGameManager.UpdatePrestigePointAllocation(nodeToUnallocate.AllocationId, nodeToUnallocate.Level - 1, cost);
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
        
        protected virtual int GetNodeCost(string allocationId, int level)
        {
            MetaPrestigePointAllocatable allocatable = MetaGameManager.GetPrestigePointAllocatables().Find(a => a.Id == allocationId);
            if (allocatable != null && level > 0 && level <= allocatable.levels.Count)
            {
                return allocatable.levels[level - 1].cost;
            }
            return 0;
        }

        protected int GetAvailablePrestigePoints()
        {
            MetaProgressData data = MetaGameManager.GetProgress();
            return data.prestigePoints - MetaGameManager.GetAllocatedPrestigePointCount();
        }
    }
}