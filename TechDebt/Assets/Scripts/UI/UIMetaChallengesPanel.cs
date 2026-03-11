using UnityEngine;
using System.Collections.Generic;
using System.Text;
using UI;
using MetaChallenges;

public class UIMetaChallengesPanel: UIPanel
{


    public override void Show()
    {
        base.Show();

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
            }

            rowI++;
            
            int currentProgress = 0;
            var infraStats = progressData.metaStats?.infra.Find(i => i.infraId == challenge.InfrastructureId);
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

                
         
                
             
           
        }
    }

    public override void Close(bool forceClose = false)
    {
        base.Close(forceClose);
        GameManager.Instance.UIManager.mainMenu.Show();
    }
}