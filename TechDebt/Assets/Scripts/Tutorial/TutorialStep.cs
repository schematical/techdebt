using Tutorial;
using UI;

namespace Events
{
    public class TutorialStep
    {
        public TutorialStepId Id { get; private set; }
        protected string Name { get; private set; }
        protected string Description { get; private set; }
        public string spriteId = null;

        public TutorialStep(TutorialStepId id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        public void Render()
        {
            GameManager.Instance.UIManager.tutorialPanel.titleText.text = Name;
            if (spriteId != null)
            {
                UIPanelImage panelImage = GameManager.Instance.UIManager.tutorialPanel.AddLine<UIPanelImage>();
                panelImage.image.sprite = GameManager.Instance.SpriteManager.GetSprite(spriteId);
            }

            GameManager.Instance.UIManager.tutorialPanel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =
                Description;

            GameManager.Instance.UIManager.tutorialPanel.Show();
        }

        public void Trigger()
        {
            throw new System.NotImplementedException();
        }
    }
}