using DefaultNamespace;

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
                    break;
            }
        }

        void AddTutorialButton()
        {
            AddButton("My First Website", () =>
            {
                
            });
        }
        // "My First Website";
    }
}