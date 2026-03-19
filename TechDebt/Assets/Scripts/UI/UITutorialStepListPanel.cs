using Tutorial;

namespace UI
{
    public class UITutorialStepListPanel: UIPanel
    {
        public override void Show()
        {
            base.Show();
            foreach (TutorialStep step in GameManager.Instance.TutorialManager.GetSteps())
            {
                UIPanelLine line = AddLine<UIPanelLine>();
                switch (step.State)
                {
                    case(TutorialStep.TutorialStepState.Incomplete) :
                        break;
                    default:
                        line.Add<UIPanelLineSectionText>().text.text = $"{step.Id} - {step.State}";
                        break;
                }
             
            }
        }
    }
}