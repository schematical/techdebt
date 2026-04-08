using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tutorial;

namespace UI
{
    public class UITechTreePanel : UIMapPanel
    {
        public override void PopulateNodes()
        {
            List<Technology> allTech = GameManager.Instance.GetAllTechnologies();
            foreach (Technology tech in allTech)
            {
                _mapNodes.Add(new MapNodeView { Node = tech });
            }
        }

        public override void Show()
        {
            base.Show();
            GameManager.Instance.UIManager.ForcePause();
            GameManager.OnTechnologyStateChange += OnTechnologyStateChange;
        }

        public override void Close(bool forceClose = false)
        {
            if (panelState != UIState.Closed)
            {
                GameManager.Instance.UIManager.StopForcePause();
            }
            GameManager.OnTechnologyStateChange -= OnTechnologyStateChange;
            base.Close(forceClose);
        }

        public void OnTechnologyStateChange(Technology technology, Technology.State previousState)
        {
            Refresh();
        }

        protected override void SelectNode(MapNodeView nodeView)
        {
            if (_selectedNode == nodeView)
            {
                Technology tech = (Technology)nodeView.Node;
                // Double click or click while selected - try to research
                if (tech.CurrentState == Technology.State.Locked)
                {
                    GameManager.Instance.SelectTechnologyForResearch(tech);
                    Refresh();
                }
                Close();
            }

            base.SelectNode(nodeView);
        }

        public override void UpdateDetailsArea()
        {
            CleanUp();
            if (_selectedNode == null)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "Select a technology to see details.";
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =
                    "Unlock more tech by completing Challenges. \n\n Read more in the Challenges section of the Main Menu inbetween runs.";
                return;
            }

            Technology tech = (Technology)_selectedNode.Node;
            UIPanelLineSectionText header = AddLine<UIPanelLine>().Add<UIPanelLineSectionText>();
            header.h1(tech.DisplayName);

            string description = tech.Description; 

            if (tech.TutorialStepId != TutorialStepId.None)
            {
                TutorialStep step = GameManager.Instance.TutorialManager.GetStep(tech.TutorialStepId);
                if (step.spriteId != null)
                {
                    AddLine<UIPanelImage>().image.sprite = GameManager.Instance.SpriteManager.GetSprite(step.spriteId);
                }
                description = step.Description;
            }
            
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"{description}";            
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "\n";
            
            float researchDuration = (tech.ResearchTime / GameManager.Instance.GameLoopManager.GetDayDurationSeconds()) * 8;
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =  $"Research Time: {Math.Round(researchDuration * 100)/100} Hours\n";

            if (tech.UnlockConditions != null && tech.UnlockConditions.Count > 0)
            {
                List<string> reqs = new List<string>();
                foreach (UnlockCondition unlockCondition in tech.UnlockConditions)
                {
                    if (unlockCondition.Type == UnlockCondition.ConditionType.Technology)
                    {
                        reqs.Add(unlockCondition.ToString());
                    }
                }
                if (reqs.Count > 0)
                    AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =  $"Requires: {string.Join(", ", reqs)}";
            }

            if (tech.CurrentState == Technology.State.Locked)
            {
                bool prerequisitesMet = tech.UnlockConditions?.All(condition => condition.IsUnlocked()) ?? true;
                if (!prerequisitesMet)
                {
                    AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =  "Prerequisites not met.";
                }
                else
                {
                    AddButton("Start Research", () => { 
                        GameManager.Instance.SelectTechnologyForResearch(tech);
                        Refresh();
                    });
                }
            }
            else if (tech.CurrentState == Technology.State.Researching)
            {
                float percentage = tech.GetProgress() * 100f;
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =  $"Researching: {Mathf.FloorToInt(percentage)}%";
            }
            else if (tech.CurrentState == Technology.State.Unlocked)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =  "Fully Researched";
            }
        }
    }
}