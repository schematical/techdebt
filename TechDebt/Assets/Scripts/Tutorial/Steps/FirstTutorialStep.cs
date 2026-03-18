using System.Collections.Generic;

namespace Tutorial.Steps
{
    public class FirstTutorialStep: TutorialStep
    {
        public FirstTutorialStep(TutorialStepId id, string name, string description) : base(id, name, description)
        {
        }

        public override List<DialogButtonOption> GetDialogOptions()
        {
            return new List<DialogButtonOption>()
            {
                new DialogButtonOption() { Text = "Start Tutorial", OnClick = () =>
                    {
                        GameManager.Instance.HideAllAttentionIcons();
                        Next();
                    } 
                },
                new DialogButtonOption() { Text = "Just Get Started", OnClick = () => GameManager.Instance.TutorialManager.End() },
            };
        }
    }
}