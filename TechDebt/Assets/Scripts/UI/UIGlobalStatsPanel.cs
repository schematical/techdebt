using NPCs;
using Stats;

namespace UI
{
    public class UIGlobalStatsPanel: UIPanel
    {
        public UITextArea textArea;
        public void Show()
        {
            base.Show();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<b>Stats</b>");
            foreach (var stat in GameManager.Instance.Stats.Stats)
            {
                sb.AppendLine($" - {stat.Key}: B: {stat.Value.BaseValue}/V: {stat.Value.Value}");
                foreach (StatModifier modifier in stat.Value.Modifiers)
                {
                    sb.AppendLine($"   -  {modifier.Id} - {modifier.Type}: {modifier.GetDisplayText()}");
                }
            }
            
            sb.AppendLine("\n\n\n<b>Modifiers</b>");
            foreach (ModifierBase modifier in GameManager.Instance.Modifiers.Modifiers)
            {
                sb.AppendLine(modifier.ToFullDetail());
            }

            textArea.textArea.text = sb.ToString();
        }
    }
}