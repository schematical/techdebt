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
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public MapNodeDirection Direction { get; set; }
        public List<string> DependencyIds { get; set; }
        public int PrestigeCost { get; set; }
        public StatType StatType;
        public float Value;

        public MapNodeState CurrentState
        {
            get
            {
                if (MetaGameManager.IsResourceEquipped(ResourceType, Id))
                    return MapNodeState.Unlocked;

                // For Meta nodes, visibility is handled by PopulateNodes, 
                // but we still need to know if it's "Ready to purchase" (Locked) or "Prereqs not met" (MetaLocked)
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

    public class UIMetaUnlockMapPanel : UIMapPanel
    {
        public enum MetaUnlockTab { Bonuses, Technology }
        private MetaUnlockTab _currentTab = MetaUnlockTab.Technology;
        public Transform metaUnlockMapTabs;

        public override void Show()
        {
            base.Show();
            CreateTabs();
        }

        private void CreateTabs()
        {

            metaUnlockMapTabs.gameObject.SetActive(true);
            AddTabButton("Technology", MetaUnlockTab.Technology);
            AddTabButton("Bonuses", MetaUnlockTab.Bonuses);
        }

        private void AddTabButton(string label, MetaUnlockTab tab)
        {
            GameObject btnGO = GameManager.Instance.prefabManager.Create("UIButton", Vector3.zero, metaUnlockMapTabs);
            UIButton uiBtn = btnGO.GetComponent<UIButton>();
            uiBtn.buttonText.text = label;
            uiBtn.button.onClick.AddListener(() => SwitchTab(tab));
        }

        public void SwitchTab(MetaUnlockTab tab)
        {
            _currentTab = tab;
            _selectedNode = null;
            Refresh();
        }

        public override void PopulateNodes()
        {
            if (_currentTab == MetaUnlockTab.Bonuses)
            {
                foreach (UIMetaUnlockMapNode node in GetMetaUnlockDefinitions())
                {
                    _mapNodes.Add(new MapNodeView { Node = node });
                }
            }
            else if (_currentTab == MetaUnlockTab.Technology)
            {
                List<Technology> allTech = MetaGameManager.GetAllTechnologies();
                foreach (Technology tech in allTech)
                {
                    // Same visibility logic as Tech Tree: show if dependencies are met OR it's a root node
                    bool isVisable = tech.UnlockConditions.All(condition =>
                    {
                        if (condition.Type != UnlockCondition.ConditionType.Technology) return true;
                        // For the meta panel, we check if it exists in our static tech list
                        return allTech.Any(t => t.TechnologyID == condition.TechnologyID);
                    });

                    if (isVisable)
                    {
                        int prestigeCost = Mathf.CeilToInt(tech.ResearchTime / 30f);
                        UIMetaUnlockMapNode node = new UIMetaUnlockMapNode
                        {
                            ResourceType = MetaResourceType.Technology,
                            Id = tech.TechnologyID,
                            DisplayName = tech.DisplayName,
                            Description = tech.Description,
                            Direction = (MapNodeDirection)tech.Direction,
                            DependencyIds = tech.DependencyIds,
                            PrestigeCost = prestigeCost
                        };
                        _mapNodes.Add(new MapNodeView { Node = node });
                    }
                }
            }
        }

        protected override bool IsNodeVisible(MapNodeView nodeView)
        {
            // In the meta panel, we want to see the whole tree for that tab
            return true;
        }

        public override void UpdateDetailsArea()
        {
            CleanUp();
            
            MetaProgressData progress = MetaGameManager.LoadProgress();
            UIPanelLine prestigeLine = AddLine<UIPanelLine>();
            prestigeLine.Add<UIPanelLineSectionText>().text.text = $"Vested Shares: {progress.prestigePoints}";

            if (_selectedNode == null)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "Select a node to allocate Vested Shares.";
                return;
            }

            UIMetaUnlockMapNode mapNode = (UIMetaUnlockMapNode)_selectedNode.Node;
            UIPanelLineSectionText header = AddLine<UIPanelLine>().Add<UIPanelLineSectionText>();
            header.h1(mapNode.DisplayName);
            
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = mapNode.Description;
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"\nCost: {mapNode.PrestigeCost} Vested Shares";

            // Display Unlock Requirements
            if (mapNode.DependencyIds != null && mapNode.DependencyIds.Count > 0)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "\nRequirements:";
                foreach (string depId in mapNode.DependencyIds)
                {
                    // Try to find the name of the dependency
                    string depName = depId;
                    if (mapNode.ResourceType == MetaResourceType.Technology)
                    {
                        Technology depTech = MetaGameManager.GetAllTechnologies().Find(t => t.TechnologyID == depId);
                        if (depTech != null) depName = depTech.DisplayName;
                    }

                    bool met = MetaGameManager.IsResourceEquipped(mapNode.ResourceType, depId);
                    string status = met ? "<color=green>(MET)</color>" : "<color=red>(NOT MET)</color>";
                    AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $" - {depName} {status}";
                }
            }

            bool isEquipped = MetaGameManager.IsResourceEquipped(mapNode.ResourceType, mapNode.Id);

            if (isEquipped)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "\nSTATUS: ALLOCATED (START UNLOCKED)";
                
                // Check for allocated dependents to show a warning
                List<MetaUnlockResource> allocatedDependents = progress.prestigePointAllocations.FindAll(r => {
                    if (r.Type != mapNode.ResourceType) return false;
                    if (r.Type == MetaResourceType.Technology)
                    {
                        Technology tech = MetaGameManager.GetAllTechnologies().Find(t => t.TechnologyID == r.Id);
                        return tech != null && tech.DependencyIds != null && tech.DependencyIds.Contains(mapNode.Id);
                    }
                    return false;
                });

                if (allocatedDependents.Count > 0)
                {
                    string depNames = string.Join(", ", allocatedDependents.Select(d => {
                        Technology t = MetaGameManager.GetAllTechnologies().Find(tech => tech.TechnologyID == d.Id);
                        return t != null ? t.DisplayName : d.Id;
                    }));
                    AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"<color=orange>Warning: Unallocating this will also refund dependents: {depNames}</color>";
                }

                AddButton("Unallocate (Refund)", () => {
                    UnallocateRecursive(mapNode.ResourceType, mapNode.Id, mapNode.PrestigeCost);
                    Refresh();
                });
            }
            else
            {
                if (mapNode.CurrentState == MapNodeState.Locked)
                {
                    if (progress.prestigePoints >= mapNode.PrestigeCost)
                    {
                        AddButton("Allocate", () => {
                            MetaGameManager.ToggleResourceEquip(mapNode.ResourceType, mapNode.Id, mapNode.PrestigeCost, mapNode.StatType, mapNode.Value);
                            Refresh();
                        });
                    }
                    else
                    {
                        AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "\nNOT ENOUGH VESTED SHARES";
                    }
                }
                else
                {
                    AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "\nPREREQUISITES NOT MET";
                }
            }
        }

        private void UnallocateRecursive(MetaResourceType type, string id, int cost)
        {
            MetaProgressData progress = MetaGameManager.LoadProgress();
            
            // Find everything that depends on this resource
            List<MetaUnlockResource> dependents = progress.prestigePointAllocations.FindAll(r => {
                if (r.Type != type) return false;
                if (r.Type == MetaResourceType.Technology)
                {
                    Technology tech = MetaGameManager.GetAllTechnologies().Find(t => t.TechnologyID == r.Id);
                    return tech != null && tech.DependencyIds != null && tech.DependencyIds.Contains(id);
                }
                if (r.Type == MetaResourceType.GlobalStat)
                {
                    UIMetaUnlockMapNode node = GetMetaUnlockDefinitions().Find(n => n.Id == r.Id);
                    return node != null && node.DependencyIds != null && node.DependencyIds.Contains(id);
                }
                return false;
            });

            foreach (MetaUnlockResource dep in dependents)
            {
                int depCost = 0;
                if (dep.Type == MetaResourceType.Technology)
                {
                    Technology depTech = MetaGameManager.GetAllTechnologies().Find(t => t.TechnologyID == dep.Id);
                    depCost = depTech != null ? Mathf.CeilToInt(depTech.ResearchTime / 30f) : 0;
                }
                else if (dep.Type == MetaResourceType.GlobalStat)
                {
                    UIMetaUnlockMapNode depNode = GetMetaUnlockDefinitions().Find(n => n.Id == dep.Id);
                    depCost = depNode != null ? depNode.PrestigeCost : 0;
                }
                UnallocateRecursive(dep.Type, dep.Id, depCost);
            }

            MetaGameManager.ToggleResourceEquip(type, id, cost);
        }

        public override void Close(bool forceClose = false)
        {
            base.Close(forceClose);
            
            metaUnlockMapTabs.gameObject.SetActive(false);
            if (GameManager.Instance.State == GameManager.GameManagerState.MainMenu)
            {
                GameManager.Instance.UIManager.saveSlotDetailPanel.Show();
            }
        }

        private List<UIMetaUnlockMapNode> GetMetaUnlockDefinitions()
        {
            List<UIMetaUnlockMapNode> nodes = new List<UIMetaUnlockMapNode>();

            // --- Upward Tree (Money) ---
            float[] moneyValues = { 50, 100, 200, 400, 800, 1600 };
            for (int i = 1; i <= 6; i++)
            {
                nodes.Add(new UIMetaUnlockMapNode
                {
                    ResourceType = MetaResourceType.GlobalStat,
                    Id = $"money-{i}",
                    DisplayName = $"Investment Level {i}",
                    Description = $"Start each run with an additional ${moneyValues[i-1]}.",
                    PrestigeCost = i,
                    Direction = MapNodeDirection.Up,
                    DependencyIds = i == 1 ? new List<string>() : new List<string> { $"money-{i-1}" },
                    StatType = StatType.Money,
                    Value = moneyValues[i-1]
                });
            }

            // --- Downward Tree (ReRolls & Banish) ---
            // Root: ReRoll 1
          

            // Subsequent ReRolls (2-6)
            for (int i = 1; i <= 6; i++)
            {
                UIMetaUnlockMapNode node = new UIMetaUnlockMapNode
                {
                    ResourceType = MetaResourceType.GlobalStat,
                    Id = $"reroll-{i}",
                    DisplayName = $"ReRoll {i}",
                    Description = $"Gain an additional Re-Roll (Total: {i}).",
                    PrestigeCost = i,
                    Direction = MapNodeDirection.Down,
                    DependencyIds = new List<string> { $"reroll-{i - 1}" },
                    StatType = StatType.Global_ReRolls,
                    Value = 1
                };
                nodes.Add(node);
            }

            // Subsequent Banishes (2-6)
            for (int i = 1; i <= 6; i++)
            {
                nodes.Add(new UIMetaUnlockMapNode
                {
                    ResourceType = MetaResourceType.GlobalStat,
                    Id = $"banish-{i}",
                    DisplayName = $"Banish Level {i}",
                    Description = $"Gain an additional Banish (Total: {i}).",
                    PrestigeCost = i,
                    Direction = MapNodeDirection.Down,
                    DependencyIds = new List<string> { $"banish-{i-1}" },
                    StatType = StatType.Global_Banish,
                    Value = 1
                });
            }

            return nodes;
        }
    }
}