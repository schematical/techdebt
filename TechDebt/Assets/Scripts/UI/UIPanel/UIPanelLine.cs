using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIPanelLine: MonoBehaviour
    {
        
        public HorizontalLayoutGroup hozLayoutGroup;
        public VerticalLayoutGroup vertLayoutGroup;
        protected List<UIPanelLineSection> sections = new List<UIPanelLineSection>();
        protected List<UIPanelLine> lines = new List<UIPanelLine>();
        public T Add<T>(UIPanelLineSectionOptions options) where T:  UIPanelLineSection
        {
            string prefabId = typeof(T).Name;
      
            T section =
                GameManager.Instance.prefabManager.Create(prefabId, Vector3.zero, hozLayoutGroup.transform)
                    .GetComponent<T>();
            if (section == null)
            {
                throw new SystemException($"Cannot find `{prefabId}`'s component of same type");
            }
            section.Initialize(options);
            sections.Add(section);
            return section;
        }

        public void CleanUp()
        {
            foreach (UIPanelLineSection section in sections)
            {
                section.gameObject.SetActive(false);
            }
            foreach (UIPanelLine line in lines)
            {
                line.CleanUp();
            }
            gameObject.SetActive(false);
        }
        public UIPanelLine AddLine()
        {
            UIPanelLine panelLine =
                GameManager.Instance.prefabManager.Create("UIPanelLine", Vector3.zero, vertLayoutGroup.transform)
                    .GetComponent<UIPanelLine>();
            lines.Add(panelLine);
            return panelLine;
        }

        public void ClearChildLines()
        {
            foreach (UIPanelLine line in lines)
            {
                line.ClearChildLines();
                line.CleanUp();
            }
            lines.Clear();
        }

        public List<UIPanelLine> GetLines()
        {
            return lines;
        }
    }
 
    public class UIPanelLineSectionOptions
    {
        public string text = "";
        public int fontSize = 22;
        public int width;
        public Color textColor = Color.white;
        public Sprite sprite;

        public Action<UIPanelLineSectionButton> onClick;
    }

   
}