using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        public Action<UIPanelLine> onExpand;
        protected int depth = 0;
        protected UIPanel rootPanel;
        protected UIPanelLine parentLine;

        public void Initialize(int _depth, UIPanel _rootPanel, UIPanelLine _parentLine)
        {
            depth = _depth;
            rootPanel = _rootPanel;
            parentLine = _parentLine;
            
            UIPanelLineSectionText sectionText = Add<UIPanelLineSectionText>();
            sectionText.text.text = "";// depth.ToString();
            // sectionText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, depth * 20);
            sectionText.GetComponent<LayoutElement>().preferredWidth = depth * 20;
        
        }
        public T Add<T>() where T:  UIPanelLineSection
        {
            string prefabId = typeof(T).Name;
      
            T section =
                GameManager.Instance.prefabManager.Create(prefabId, Vector3.zero, hozLayoutGroup.transform)
                    .GetComponent<T>();
            if (section == null)
            {
                throw new SystemException($"Cannot find `{prefabId}`'s component of same type");
            }
            section.Initialize();
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
        public T AddLine<T>()  where T:  UIPanelLine
        {
            string prefabId = typeof(T).Name;
            T panelLine =
                GameManager.Instance.prefabManager.Create(prefabId, Vector3.zero, vertLayoutGroup.transform)
                    .GetComponent<T>();
            panelLine.Initialize(depth + 1, rootPanel,  this);
            lines.Add(panelLine);
            return panelLine;
        }

        public void SetRootPanel(UIPanel _rootPanel)
        {
            rootPanel = _rootPanel;
        }

        public void ClearChildLines()
        {
            foreach (UIPanelLine line in lines)
            {
                line.ClearChildLines();
                line.CleanUp();
            }
            lines.Clear();
            rootPanel.Refresh();
        }

        public List<UIPanelLine> GetLines()
        {
            return lines;
        }

        public void SetExpandable(Action<UIPanelLine> _onExpand)
        {
            onExpand = _onExpand;
            if (onExpand == null)
            {
                Debug.LogError("TODO: Write this");
                return;
            }

            UIPanelLineSectionButton button = Add<UIPanelLineSectionButton>();
            button.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            button.AddComponent<LayoutElement>().preferredWidth = 25;
            button.text.text = "+";
            button.button.onClick.AddListener( () =>
            {
                bool isExpanded = GetLines().Count != 0;
                if (isExpanded)
                {
                    button.text.text = "+";
                    ClearChildLines();
                    return;
                } 
                button.text.text = "-";
                onExpand.Invoke(this);
                rootPanel.Refresh();
            });

        }

        public void Refresh()
        {
            foreach (UIPanelLine line in lines)
            {
                line.Refresh();
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(vertLayoutGroup.GetComponent<RectTransform>());
        }
    }
 
   

   
}