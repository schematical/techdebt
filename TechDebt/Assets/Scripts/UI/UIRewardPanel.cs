using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class UIRewardPanel : MonoBehaviour
    {
        public enum State
        {
            Closed,
            Opened
        };
        private State state = State.Closed;
        public Button openButton;
        public Image rewardImage;
        public TextMeshProUGUI primaryText;
        public TextMeshProUGUI secondaryText;
        private UnityAction onDone;
        private ReleaseBase release;
        public Image panelImage;
        void Start()
        {
            openButton.onClick.AddListener(OnOpenClick);
        }
        public void Show(ReleaseBase releaseBase)
        {
           release = releaseBase;
           gameObject.SetActive(true);
           rewardImage.gameObject.SetActive(false);
           primaryText.gameObject.SetActive(false);
           secondaryText.gameObject.SetActive(false);
           openButton.gameObject.SetActive(true);
           panelImage.color = new Color(0, 0, 0, 0);

        }

        
        public void OnOpenClick()
        {
            
            state = State.Opened;
            panelImage.color = Color.white;
            rewardImage.gameObject.SetActive(true);
            primaryText.gameObject.SetActive(true);
            primaryText.text = release.RewardModifier.GetTitle();
            secondaryText.gameObject.SetActive(true);
            secondaryText.text = release.RewardModifier.GetNextLevelUpDisplayText(Rarity.Common);
            openButton.gameObject.SetActive(false);
            GameManager.Instance.UIManager.SetTimeScalePause();
            Sprite icon = GameManager.Instance.SpriteManager.GetSprite(release.RewardModifier.IconPrefab);
            rewardImage.sprite = RarityHelper.PaintIcon(Rarity.Common, icon);
            Debug.Log($"Showing {release.rewardRarity}");
            foreach (ApplicationServer applicationServer in release.GetAllReleaseTargets())
            {
                applicationServer.ShowLevelUpGraphic(release.rewardRarity, (currentlyDisplayedRarity, isDone) =>
                {
                    if (isDone)
                    {
                        release.OnDeploymentCompleted();
                        gameObject.SetActive(false);
                        GameManager.Instance.UIManager.Resume();
                        if (GameManager.Instance.Tutorial != null)
                        {
                            GameManager.Instance.Tutorial.OnRewardsPanelDone();
                        }

                        return;
                    }
                    rewardImage.sprite = RarityHelper.PaintIcon(currentlyDisplayedRarity, icon);
                    secondaryText.text = release.RewardModifier.GetNextLevelUpDisplayText(currentlyDisplayedRarity);
                });
            }
         
            
        }
    }
}