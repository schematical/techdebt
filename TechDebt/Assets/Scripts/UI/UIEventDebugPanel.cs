using System.Collections.Generic;
using System.Linq;
using Tutorial;
using UnityEngine;

namespace UI
{
    public class UIEventDebugPanel: UIPanel
    {
        //public Dictionary<string, UITextArea> textAreas = new Dictionary<string, UITextArea>();
        public enum FieldType { Name, Probability }
        public override void Show()
        {
            base.Show();
            foreach (EventBase eventBase in GameManager.Instance.Events)
            {
                UIPanelLine line = AddLine<UIPanelLine>();
                line.SetId(eventBase.GetName());
                UIPanelLineSectionText textSection = line.Add<UIPanelLineSectionText>();
                textSection.SetId(FieldType.Name.ToString());
                textSection.text.text = eventBase.GetName();
                
                UIPanelLineSectionText probSection = line.Add<UIPanelLineSectionText>();
                probSection.SetId(FieldType.Probability.ToString());
                probSection.text.text = eventBase.GetProbability().ToString();
                UIPanelLineSectionButton button = line.Add<UIPanelLineSectionButton>();
                button.text.text = "Trigger";
                button.button.onClick.AddListener(() =>
                {
                    GameManager.Instance.TriggerEvent(eventBase);
                });
                /*line.SetExpandable((UIPanelLine line) =>
                {
                    eventBase.Render(line);
                });*/
            }
        }
        void Update()
        {
            // Clear existing entries
           base.Update();
            if(lines.Count == 0) return;

         
            foreach (EventBase eventBase in GameManager.Instance.Events)
            {
                UIPanelLine line = GetLineById(eventBase.GetName());
                if (line == null)
                {
                    Debug.LogError($"{eventBase.GetName()} - not found in lines. Count: {lines.Count}");
                    continue;
                }
                line.GetSectionById<UIPanelLineSectionText>(FieldType.Probability.ToString()).text.text = eventBase.GetProbability().ToString();
            }
        }
    }
}