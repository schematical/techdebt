using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UI;
using UnityEditor;
using UnityEngine.Serialization;
using UnityEngine.UnityConsent;

public class UIMainMenu : UIPanel
{
    public override void Show()
    {
        base.Show();
        GameManager.Instance.UIManager.Block();
        
        AddButton("Play", () => { Close(); GameManager.Instance.UIManager.saveSlotListPanel.Show(); });
        AddButton("Discord", OpenDiscord);     
        AddButton("Quit", () => { Application.Quit(); });
        AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Play Test - v{Application.version}";
    }

    private void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/yFDKNDBquZ");
    }


    public void NewGame()
    {
        Close();
        EndUserConsent.SetConsentState(new ConsentState {
            AnalyticsIntent = ConsentStatus.Granted,
            // AdsIntent = ConsentStatus.Denied
        });
        GameManager.Instance.StartNewGame();
    }


    public void ShowChallengesPanel()
    {
        Close();
        GameManager.Instance.UIManager.metaChallengesPanel.Show();
    }
}