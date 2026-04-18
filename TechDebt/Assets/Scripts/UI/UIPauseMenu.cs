using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UI;
using UnityEditor;
using UnityEngine.Serialization;

public class UIPauseMenu : UIPanel
{
    public override void Show()
    {
        base.Show();
        GameManager.Instance.UIManager.Block();
        AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"v{Application.version}";
        AddButton("Resume", () => Close());
        AddButton("Challenges", ShowChallengesPanel);
        AddButton("Quit", Quit);
    }

    private void Quit()
    {
        GameManager.Instance.Reset();
        Close();
        GameManager.Instance.ShowSaveSlotDetailPanel();
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