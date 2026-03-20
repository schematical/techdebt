using System;
using System.Collections.Generic;
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
        public TutorialStepId NextStepId = TutorialStepId.None;
        public bool forcePause = true;
 

        public TutorialStep(TutorialStepId id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
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
                // GameManager.Instance.UIManager.ForcePause();
            }
            NPCBase npc = GetSpeaker();
            UIDialogBubble dialogBubble = npc.ShowDialogBubble();
            dialogBubble.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = Description;
            GameManager.Instance.cameraController
                .ZoomToAndFollow(npc.transform);
            if (getTarget != null)// TargetSelector != null)
            {
                // GameManager.Instance.cameraController
                    // .ZoomToAndFollow(getTarget()); // TargetSelector.GetTransform());
                    npc.AssignTask(
                        new TutorialMoveToTask(this)    
                    );
            }

            foreach (DialogButtonOption option in GetDialogOptions())
            {
                dialogBubble.AddButton(
                    option.Text,
                    () =>
                    {
                        npc.HideDialogBubble();
                        option.OnClick.Invoke();
                    }
                );
            }
            /*GameManager.Instance.UIManager.ShowNPCDialog(
                GameManager.Instance.SpriteManager.GetSprite(spriteId),
                Description,
                GetDialogOptions()
            );*/
        }

        public virtual List<DialogButtonOption> GetDialogOptions()
        {
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
            State = TutorialStepState.Completed;
            GameManager.Instance.TutorialManager.Next(NextStepId);
        }

       
        public void Trigger()
        {
            if (State != TutorialStepState.Incomplete)
            {
                return;// Debug.LogWarning($"TutorialStep {Id} - Trying to Trigger but state is {State}");
            }
            State = TutorialStepState.InProgress;
            if (onTrigger != null)
            {
                onTrigger.Invoke();
            }
            Render();
        }
    }
}