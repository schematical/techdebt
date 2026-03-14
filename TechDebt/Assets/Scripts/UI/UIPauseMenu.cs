using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UI;
using UnityEngine.Serialization;

public class UIPauseMenu : UIPanel
{
    public override void Show()
    {
        base.Show();
        GameManager.Instance.UIManager.Block();
        AddButton("Resume", () => Close());
        AddButton("Challenges", ShowChallengesPanel);
        AddButton("Quit", Quit);
    }

    private void Quit()
    {
        GameManager.Instance.Reset();
        GameManager.Instance.ShowMainMenu();
    }


    public void ShowChallengesPanel()
    {
        Close();
        GameManager.Instance.UIManager.metaChallengesPanel.Show();
    }
    public override void Close(bool forceClose = false)
    {
        base.Close(forceClose);
        GameManager.Instance.UIManager.RemoveBlock();
    }
}