using UnityEngine;
using System.Collections.Generic;
using System.Text;
using UI;
using MetaChallenges;

public class UIMetaChallengesPanel: UIPanel
{
    protected List<Transform> rows = new List<Transform>();
    protected List<UIChallengeSelectPanel> challengePanels = new List<UIChallengeSelectPanel>();

    public override void Show()
    {
        base.Show();
        GameManager.Instance.UIManager.Block();
        // Ensure MetaProgressData is loaded before trying to access metaStats
        MetaProgressData progressData = MetaGameManager.ProgressData;
        int rowI = 0;
        Transform row = null;
        List<MetaChallengeBase> challenges = MetaGameManager.GetAllChallenges();
      
        foreach (MetaChallengeBase challenge in challenges)
        {
   
            if (rowI % 5 == 0)
            {
                //Add a new row
                row = GameManager.Instance.prefabManager.Create("UIGridRow", new Vector3(), scrollContent).GetComponent<Transform>();
                rows.Add(row);
            }

            rowI++;
            
            int currentProgress = 0;
            var infraStats = progressData.metaStats?.infra.Find(i => i.infraId == challenge.WorldObjectTypeId);
            if (infraStats != null)
            {
                var statPair = infraStats.stats.Find(s => s.statName == challenge.metaStat.ToString());
                if (statPair != null)
                {
                    currentProgress = statPair.cumulativeValue;
                }
            }

            UIChallengeSelectPanel challengePanel = GameManager.Instance.prefabManager.Create(
                "UIChallengeSelectPanel",
                Vector3.zero, 
                row
                ).GetComponent<UIChallengeSelectPanel>();
           
            challengePanel.Initialize(challenge, currentProgress);
            challengePanels.Add(challengePanel);
        }
    }

    public override void Close(bool forceClose = false)
    {
        foreach (UIChallengeSelectPanel challengePanel in challengePanels)
        {
            challengePanel.gameObject.SetActive(false);
        }
        foreach (Transform row in rows)
        {
            row.gameObject.SetActive(false);
        }
        base.Close(forceClose);
        switch (GameManager.Instance.State)
        {
            case(GameManager.GameManagerState.MainMenu):
                GameManager.Instance.UIManager.mainMenu.Show();
                break;
            case(GameManager.GameManagerState.Playing):
                GameManager.Instance.UIManager.pauseMenu.Show();
                break;
            default:
                throw new System.NotImplementedException();
        }

    }
}