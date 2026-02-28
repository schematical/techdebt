using NPCs;
using Stats;

namespace UI
{
    public class UIGlobalStatsPanel: UIPanel
    {
        public UITextArea textArea;
        public override void Show()
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
            sb.AppendLine("\n\n\n<b>Network Packets</b> \n");
            foreach (NetworkPacketData networkPacketData in GameManager.Instance.GetNetworkPacketDatas())
            {
                sb.AppendLine($"- {networkPacketData.Type}");
                foreach (StatType statType in networkPacketData.Stats.Stats.Keys)
                {
                    StatData statData = networkPacketData.Stats.Stats[statType];
                    sb.AppendLine($"   - {statType} - {statData.Value}");
                    foreach (StatModifier modifier in statData.Modifiers)
                    {
                        sb.AppendLine($"      - {modifier.Id} - {modifier.GetDisplayText()}");
                    }
                }
            }
            textArea.textArea.text = sb.ToString();
        }
    }
}