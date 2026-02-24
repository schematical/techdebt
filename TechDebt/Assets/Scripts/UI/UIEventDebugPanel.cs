using System.Collections.Generic;
using System.Linq;
using Events;
using UnityEngine;

namespace UI
{
    public class UIEventDebugPanel: UIPanel
    {
        //public Dictionary<string, UITextArea> textAreas = new Dictionary<string, UITextArea>();
        public UITextArea textArea;


        void Update()
        {
            // Clear existing entries
           base.Update();


            string text = "";
            foreach (EventBase eventBase in GameManager.Instance.Events)
            {
              
                text += eventBase.GetDescription() + "\n";
            }
            textArea.textArea.text = text;

            /*foreach (EventBase eventBase in GameManager.Instance.Events)
            {
                if (!textAreas.ContainsKey(eventBase.GetType().Name))
                {
                    textAreas[eventBase.GetType().Name] = GameManager.Instance.prefabManager.Create("UITextArea", Vector3.zero, scrollContent.transform).GetComponent<UITextArea>();
                }
                textAreas[eventBase.GetType().Name].textArea.text = eventBase.GetDescription();
            }*/
        }
    }
}