using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class UIRewardPanel : UIGameObject
    {
        public enum State
        {
            Closed,
            Opened,
            Done
        };

        private State panelState = State.Closed;
        public Button openButton;
        public Image rewardImage;
        public TextMeshProUGUI primaryText;
        public TextMeshProUGUI secondaryText;
        private UnityAction onDone;
        private ReleaseBase release;
        public Image panelImage;
        public Button panelButton;

        protected override void Start()
        {
            base.Start();
            openButton.onClick.AddListener(OnOpenClick);
            panelButton.onClick.AddListener(OnPanelClick);
        }

        public void Show(ReleaseBase releaseBase)
        {
            base.Show();
            panelState = State.Closed;
            release = releaseBase;
            gameObject.SetActive(true);
            rewardImage.gameObject.SetActive(false);
            primaryText.gameObject.SetActive(false);
            secondaryText.gameObject.SetActive(false);
            openButton.gameObject.SetActive(true);
            panelImage.color = new Color(0, 0, 0, 0);
        }

        public void OnPanelClick()
        {
            Finish();
        }

        public void OnOpenClick()
        {
            panelState = State.Opened;
            panelImage.color = Color.white;
            rewardImage.gameObject.SetActive(true);
            primaryText.gameObject.SetActive(true);
            primaryText.text = release.RewardModifier.GetTitle();
            secondaryText.gameObject.SetActive(true);
            secondaryText.text = release.RewardModifier.GetNextLevelUpDisplayText(Rarity.Common);
            openButton.gameObject.SetActive(false);
            release.NextState();
            GameManager.Instance.UIManager.SetTimeScalePause();
            Sprite icon = GameManager.Instance.SpriteManager.GetSprite(release.RewardModifier.IconPrefab);
            rewardImage.sprite = RarityHelper.PaintIcon(Rarity.Common, icon);
            foreach (ApplicationServer applicationServer in release.GetAllReleaseTargets())
            {
                applicationServer.ShowLevelUpGraphic(release.rewardRarity, (currentlyDisplayedRarity, isDone) =>
                {
                    if (isDone)
                    {
                        Finish();
                        return;
                    }
                    rewardImage.sprite = RarityHelper.PaintIcon(currentlyDisplayedRarity, icon);
                    secondaryText.text = release.RewardModifier.GetNextLevelUpDisplayText(currentlyDisplayedRarity);
                });
            }
        }

        protected void Finish()
        {
            if (panelState == State.Done)
            {
                return;
            }

            panelState = State.Done;
            release.OnDeploymentCompleted();
            GameManager.Instance.UIManager.Resume();
            if (GameManager.Instance.Tutorial != null)
            {
                GameManager.Instance.Tutorial.OnRewardsPanelDone();
            }
            Close();
          
         
        }
    }
}