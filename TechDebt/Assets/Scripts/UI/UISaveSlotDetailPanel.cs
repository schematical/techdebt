using UI;
using UnityEngine;
using System.Collections.Generic;

public class UISaveSlotDetailPanel : UIPanel
{
    public override void Show()
    {
        base.Show();
        Refresh();
    }

    public void Refresh()
    {
        CleanUp();
        MetaProgressData data = MetaGameManager.LoadProgress();
        
        AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Save Slot {MetaGameManager.CurrentSlotIndex + 1}";
        
        UIPanelLine statsLine = AddLine<UIPanelLine>();
        statsLine.Add<UIPanelLineSectionText>().text.text = $"Runs: {data.completedRuns}";
        statsLine.Add<UIPanelLineSectionText>().text.text = $"Wins: {data.successfulExits}";
        
        UIPanelLine pointsLine = AddLine<UIPanelLine>();
        pointsLine.Add<UIPanelLineSectionText>().text.text = $"Research: {data.researchPoints}";
        pointsLine.Add<UIPanelLineSectionText>().text.text = $"Prestige: {data.prestigePoints}";

        AddButton("Start Run", StartRun);
        AddButton("Challenges", ShowChallenges);
        AddButton("Unlock Map", ShowUnlockMap);
        AddButton("Back", () => { Close(); GameManager.Instance.UIManager.saveSlotListPanel.Show(); });
    }

    private void StartRun()
    {
        Close();
        GameManager.Instance.StartNewGame();
    }

    private void ShowChallenges()
    {
        // We don't close this panel so they can come back to it? 
        // Actually the standard seems to be closing the current one.
        Close();
        GameManager.Instance.UIManager.metaChallengesPanel.Show();
    }

    private void ShowUnlockMap()
    {
        Close();
        GameManager.Instance.UIManager.metaUnlockMapPanel.Show();
    }
}