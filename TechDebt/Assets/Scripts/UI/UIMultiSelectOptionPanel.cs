using System;
using DefaultNamespace.Rewards;
using Rewards;
using Stats;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class UIMultiSelectOptionPanel: MonoBehaviour
    {
        public enum InteractionType
        {
            Select,
            Preview,
            Banish
        }

        public string id;
        public Image image;
        public Image backgroundImage;
        public TextMeshProUGUI primaryText;
        public TextMeshProUGUI secondaryText;
        public TextMeshProUGUI banishButtonText;
        public Button selectButton;
        
        protected UnityAction<UIMultiSelectOptionPanel, InteractionType, string> onInteract;
        protected UIMultiSelectPanel parentPanel;
        
        public Button banishButton;
        private bool _banishable = false;

        public UIMultiSelectOptionPanel OnInteract(UnityAction<UIMultiSelectOptionPanel, InteractionType, string> action)
        {
            onInteract = action;
            return this;
        }

        public void SetReward(iModifiable target, RewardBase rewardBase, Rarity rarity)
        {
            Sprite sprite = rewardBase.GetSprite();
            Sprite spriteOut = RarityHelper.PaintIcon(rarity, sprite);
           
            string description = rewardBase.GetDescription();
    
            string toolTip = null;
            if (rewardBase is StatModifierReward)
            {
                StatModifierReward statModifierReward = (StatModifierReward)rewardBase;
                StatData statData = target.Stats.Get(statModifierReward.StatType);
                statModifierReward.PreviewLevelUp(rarity);
                StatModifier statModifier = statModifierReward.BuildStatModifier();
                
                // description = $"{statData.GetPreviewText(statModifier)}\n{description}";
                toolTip = $"{statData.GetPreviewText(statModifier)}\n\n{GameManager.Instance.localizationManager.GetStatTypeData(statModifierReward.StatType).ToolTip}";
                // Debug.Log($"{rewardBase} {description}");
            }/*
            else
            {
                toolTipListener.SetToolTip(null);
            }*/

    
           
            Initialize(
                GameManager.Instance.UIManager.multiSelectPanel, 
                rewardBase.Id, spriteOut, 
                $"{rewardBase.Name} - {rarity}",
                description,
                toolTip// modifierBase.GetDescription()
            );
        }
        public void Initialize(UIMultiSelectPanel _parentPanel, string _id, Sprite sprite, string _primaryText, string _secondaryText, string toolTip = null)
        {
            parentPanel = _parentPanel;
            id = _id;
            image.sprite = sprite;
            primaryText.text = _primaryText;
            secondaryText.text = _secondaryText;
            onInteract = null;
            UIToolTipListener toolTipListener = GetComponent<UIToolTipListener>();
            toolTipListener.SetToolTip(toolTip);
            /*image.color = Color.white;
            backgroundImage.color = Color.grey;*/
            Reset();
            _banishable = false;

            if (banishButton != null)
            {
                banishButton.gameObject.SetActive(false);
                banishButton.onClick.RemoveAllListeners();
            }

            // Clear any previous listeners and reset the selectButton state.
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener((() =>
            {
                if (onInteract != null)
                {
                    onInteract.Invoke(this, InteractionType.Preview, id);
                }

                _parentPanel.SetPreview(this);
            }));
        }

        public UIMultiSelectOptionPanel MarkBanisable()
        {
            _banishable = true;

            int banishCount = (int)GameManager.Instance.GetStatValue(StatType.Global_Banish);
            if (banishCount > 0 && banishButton != null)
            {
                banishButton.gameObject.SetActive(true);
                banishButtonText.text = $"Banish ({banishCount})";

                banishButton.onClick.RemoveAllListeners();
          
                banishButton.onClick.AddListener(() =>
                {
                    if (onInteract != null)
                    {
                        onInteract.Invoke(this, InteractionType.Banish, id);
                    }
                });
            }
            else if (banishButton != null)
            {
                banishButton.gameObject.SetActive(false);
            }
            return this;
        }

        public void SetParentBottomText(string text)
        {
            parentPanel.bottomText.text = text;
        }

        public void MarkSelected()
        {

            if (onInteract != null)
            {
                onInteract.Invoke(this, InteractionType.Select, id);
            }
        }

        public void Reset()
        {              
            backgroundImage.color = Color.grey;
            image.color = Color.white;
            selectButton.gameObject.SetActive(true);
        }
    }
}