using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UI;
using UnityEngine.Serialization;

public class UIMainMenu : UIPanel
{
    public override void Show()
    {
        base.Show();
        GameManager.Instance.UIManager.Block();
        AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"v{GameManager.V}";
        AddButton("New Game", NewGame);
        AddButton("Challenges", ShowChallengesPanel);
        AddButton("Discord", OpenDiscord);     
        AddButton("Quit", () => { Application.Quit(); });
    }

    private void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/yFDKNDBquZ");
    }


    public void NewGame()
    {
        Close();
        Debug.Log("NewGame");
        GameManager.Instance.StartNewGame();
    }


    public void ShowChallengesPanel()
    {
        Close();
        GameManager.Instance.UIManager.metaChallengesPanel.Show();
    }
}