using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using NPCs;
using Tutorial;
using UI;
using UnityEngine;
using UnityEngine.Events;

namespace Tutorial
{
    public class TutorialStep
    {
        public enum TutorialStepState
        {
            Incomplete,
            InProgress,
            Completed
        }

        public enum TutorialStateType
        {
            Dialog,
            Tip
        }
        public TutorialStepId Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        
        public TutorialStepState State { get; private set; } =  TutorialStepState.Incomplete;
        public TutorialStateType Type { get; set; } = TutorialStateType.Dialog;
        
        public string spriteId = null;
        public Func<iTargetable> getTarget = null;
        public UnityAction onTrigger = null;
        public UnityAction onFinish = null;
        public UnityAction<TutorialStep> onPreCheck = null;
        public TutorialStepId NextStepId = TutorialStepId.None;
        public bool forcePause = true;
        public bool showContinue = true;
        public List<UnlockCondition> unlockConditions = new List<UnlockCondition>();
        


        public TutorialStep(TutorialStepId id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        public bool CanBeTriggered()
        {
            return unlockConditions.All((condition => condition.IsUnlocked()));
        }

        public bool IsBlocking()
        {
            
            if (State == TutorialStepState.Completed)
            {
                return false;
            }
            return showContinue;
        }

        public virtual NPCBase GetSpeaker()
        {
            NPCBase npc =
                GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCSchematicalBot>() != null);
                      
            return npc;
        }
        
        public virtual void Render()
        {
            switch (Type)
            {
                case(TutorialStateType.Dialog):
                    RenderAsDialog();
                    break;
                case(TutorialStateType.Tip) :
                    RenderAsTip();
                    break;
                default:
                    throw new System.NotImplementedException();
            }
        }

        protected virtual void RenderAsDialog()
        {
            if (forcePause)
            {
                GameManager.Instance.UIManager.ForcePause();
            }
            
            NPCBase npc = GetSpeaker();
            UIDialogBubble dialogBubble = npc.ShowDialogBubble();
        
           
           
                
                
            UIPanelLine dialogLine = dialogBubble.AddLine<UIPanelLine>();
            if (spriteId != null)
            {
                Sprite sprite = GameManager.Instance.SpriteManager.GetSprite(spriteId);
                dialogLine.Add<UIPanelLineSectionImage>().image.sprite = sprite;
            }
            dialogLine.Add<UIPanelLineSectionText>().text.text = Description;
            foreach (DialogButtonOption option in GetDialogOptions())
            {
                dialogBubble.AddButton(
                    option.Text,
                    () =>
                    {
                        npc.HideDialogBubble();
                        GameManager.Instance.cameraController.StopFollowing();
                        option.OnClick.Invoke();
                    }
                );
            }
            
        }

        public virtual List<DialogButtonOption> GetDialogOptions()
        {
            if (!showContinue)
            {
                return new List<DialogButtonOption>();
            }
            return new List<DialogButtonOption>()
            {
                new DialogButtonOption()
                {
                    Text = "Continue",
                    OnClick = () =>
                    {
                        if (forcePause)
                        {
                            GameManager.Instance.UIManager.StopForcePause();
                        }
                        Next();
                    }
                }
            };
        }

        protected virtual void RenderAsTip() {

            GameManager.Instance.UIManager.gameTipPanel.CleanUp();
            GameManager.Instance.UIManager.gameTipPanel.titleText.text = Name;
            if (spriteId != null)
            {
                UIPanelImage panelImage = GameManager.Instance.UIManager.gameTipPanel.AddLine<UIPanelImage>();
                panelImage.image.sprite = GameManager.Instance.SpriteManager.GetSprite(spriteId);
            }

            GameManager.Instance.UIManager.gameTipPanel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =
                Description;
            if (NextStepId == null)
            {
                
            }
            GameManager.Instance.UIManager.gameTipPanel.Show();
        }

        public virtual void Next()
        {
            if (onFinish != null)
            {
                onFinish.Invoke();
            }
            MarkCompleted();
            GameManager.Instance.TutorialManager.Next(NextStepId);
        }

       
        public void Trigger()
        {
            if (State != TutorialStepState.Incomplete)
            {
                // Debug.LogWarning($"TutorialStep {Id} - Trying to Trigger but state is {State}");
                return;
            }
            State = TutorialStepState.InProgress;
            if (onTrigger != null)
            {
                onTrigger.Invoke();
            }

            if (getTarget == null)
            {
                Render();
                return;
            }
            NPCBase npc = GetSpeaker();
            npc.gameObject.SetActive(true);
            GameManager.Instance.cameraController
                .ZoomToAndFollow(npc.transform);
            GameManager.Instance.AddTask(
                new TutorialMoveToTask(this)    
            );
            
           
        }

        public void MarkCompleted()
        {
            switch (State)
            {
                case(TutorialStepState.InProgress):
                    State =  TutorialStepState.Completed;
                    break;
                default:
                    Debug.LogError($"TutorialStep: {Id} - Invalid state transition from {State} to Completed");
                    break;
            }
        }

        public TutorialStepData ToTutorialData()
        {
            return new TutorialStepData()
            {
                Id = Id,
                State = State
            };
        }

        public void FromData(TutorialStepData tutorialStepData)
        {
            State = tutorialStepData.State;
            
        }

        public void PreCheck()
        {
       
            if (onPreCheck != null)
            {
                onPreCheck.Invoke(this);
            }
        }
    }
}