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
        Debug.Log("UIMainMenu::Show");
        AddButton("New Game",  NewGame);
        AddButton("Challenges", ShowChallengesPanel);
        AddButton("Discord", OpenDiscord);
        
    }

    private void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/yFDKNDBquZ");
    }


    public void NewGame()
    {
        Close();
       GameManager.Instance.StartNewGame();
    }
    

    public void ShowChallengesPanel()
    {
        Close();
        GameManager.Instance.UIManager.metaChallengesPanel.Show();
    }  



   
}
