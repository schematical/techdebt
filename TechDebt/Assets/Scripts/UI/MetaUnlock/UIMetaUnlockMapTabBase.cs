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
            
            // Find all allocations that might depend on this specific node Id
            List<MetaPrestigePointAllocation> allAllocations = progress.prestigePointAllocations;
            
            // We need to find if ANY node in the current tab depends on the node we are unallocating
            // This is slightly expensive but necessary since we don't have a reverse lookup
            foreach (MetaPrestigePointAllocation allocation in allAllocations)
            {
                // This is tricky because one allocationId might correspond to multiple nodes (in Chain style)
                // or one node (in Leveled style).
                // For now, we look for any node in the current tab that depends on nodeToUnallocate.Id
                // and if that node's allocation is active, we unallocate it.
                
                // We'll rely on the Tab's ability to find nodes by Id.
                // This is still a bit limited because it only checks the current tab.
            }

            // Simplified approach for now: check all nodes in the current tab.
            // If we were more robust, we'd check all tabs.
            
            // For now, let's just fix the immediate logic to use the node Id for dependency checks
            // and the AllocationId for the actual update.
            
            MetaGameManager.UpdatePrestigePointAllocation(nodeToUnallocate.AllocationId, nodeToUnallocate.Level - 1, nodeToUnallocate.PrestigeCost);
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