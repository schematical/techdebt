using Stats;

namespace UI
{
    public class UIGlobalStatsPanel: UIPanel
    {
        public UITextArea textArea;
        public void Show()
        {
            gameObject.SetActive(true);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<b>Stats</b>");
            foreach (var stat in GameManager.Instance.Stats.Stats)
            {
                sb.AppendLine($"{stat.Key}: {stat.Value.Value}");
                foreach (StatModifier modifier in stat.Value.Modifiers)
                {
                    sb.AppendLine($" - {modifier.Type}: {modifier.GetDisplayText()} {modifier.Source}");
                }
            }
            
            sb.AppendLine("\n<b>Modifiers</b>");
            foreach (var modifier in GameManager.Instance.Modifiers.Modifiers)
            {
                sb.AppendLine(modifier.ToFullDetail());
            }

            textArea.textArea.text = sb.ToString();
        }
    }
}