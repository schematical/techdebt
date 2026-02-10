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
            Opened,
            Done
        };

        private State panelState = State.Closed;
        public Button openButton;
        public UIGameObject uiGameObject;
        public Image rewardImage;
        public TextMeshProUGUI primaryText;
        public TextMeshProUGUI secondaryText;
        private UnityAction onDone;
        private ReleaseBase release;
        public Image panelImage;
        public Button panelButton;

        protected void Start()
        {
          
            openButton.onClick.AddListener(OnOpenClick);
            panelButton.onClick.AddListener(OnPanelClick);
        }

        public void Show(ReleaseBase releaseBase)
        {
            panelState = State.Closed;
            release = releaseBase;
            gameObject.SetActive(true);
            uiGameObject.Close(true);
            UIButton uiButton = openButton.GetComponent<UIButton>();
            uiButton.Show();
            uiButton.Shake(5);
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
            uiGameObject.Show();
            primaryText.gameObject.SetActive(true);
            primaryText.text = release.RewardModifier.GetTitle();
            secondaryText.gameObject.SetActive(true);
            secondaryText.text = release.RewardModifier.GetNextLevelUpDisplayText(Rarity.Common);
            UIButton uiButton = openButton.GetComponent<UIButton>();
            uiButton.Close();
            release.NextState();
            GameManager.Instance.UIManager.SetTimeScalePause();
            Sprite icon = GameManager.Instance.SpriteManager.GetSprite(release.RewardModifier.IconPrefab);
            rewardImage.sprite = RarityHelper.PaintIcon(Rarity.Common, icon);
            List<ApplicationServer> targets = release.GetAllReleaseTargets();
            targets[0].ZoomTo();
            foreach (ApplicationServer applicationServer in targets)
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
            uiGameObject.Close();
          
         
        }
    }
}