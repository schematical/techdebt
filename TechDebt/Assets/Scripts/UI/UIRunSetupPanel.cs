
using System;
using DefaultNamespace;
using Stats;
using Tutorial;

namespace UI
{
    public class UIRunSetupPanel: UIPanel
    {
        // Difficulty
        
        // Show Meta Victory Conditions/Rewards 
        
        // TODO: Allow them to set their own settings but it prevents meta rewards


        public override void Show()
        {
            base.Show();
            MetaProgressData progressData = MetaGameManager.GetProgress();
            switch(progressData.gameStage)
            {
                case(GameStage.Tutorial):
                    AddTutorialButton();
                    AddButton("Skip Tutorial", () =>
                    {
                        TutorialManager tutorialManager = new TutorialManager();
                        tutorialManager.MarkTutorialDataDone();
                        MetaProgressData data = MetaGameManager.GetProgress();
                        data.gameStage = GameStage.Bootstrapped;
                        MetaGameManager.SaveProgress(data);
                        Refresh();
                        Show();
                    });
                    
                break;
                default:
                    SetupDifficultyButtons(progressData);
                break;
            }
        }

        private void SetupDifficultyButtons(MetaProgressData progressData)
        {
            foreach (GameStage stage in Enum.GetValues(typeof(GameStage)))
            {
                switch (stage)
                {
                    case(GameStage.Tutorial):
                        AddTutorialButton();
                        break;
                    default:
                        if (stage <= progressData.gameStage)
                        {
                            AddButton("Difficulty: " + stage.ToString(), () =>
                            {
                               StartNewRun(stage);
                            });
                        }

                        break;
                }
            }
         
        }

        private void StartNewRun(GameStage stage)
        {
            Close();
            GameManager.Instance.StartNewGame();
            GameManager.Instance.Map.SetDifficulty(stage);
            StatModifier difficultyStatModifier = null;
            switch (stage)
            {
                case(GameStage.Bootstrapped):
                    difficultyStatModifier =
                        new StatModifier("difficulty_" + stage, 1.1f);
                    break;
                case(GameStage.Seed):
                    difficultyStatModifier =
                        new StatModifier("difficulty_" + stage, 1.25f);
                    break;
                case(GameStage.SeriesA):
                    difficultyStatModifier =
                        new StatModifier("difficulty_" + stage, 1.5f);
                    break;
            }

            if (difficultyStatModifier != null)
            {
                GameManager.Instance.Stats.AddModifier(StatType.Difficulty, difficultyStatModifier);
            }
        }

        void AddTutorialButton()
        {
            AddButton("Tutorial", () =>
            {
                Close();
                
                GameManager.Instance.StartNewGame();
                GameManager.Instance.TutorialManager.ResetProgress();
            });
            
        }

        public void Refresh()
        {
            CleanUp();
        }

    }
}