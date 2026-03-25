using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIPanelLine: MonoBehaviour
    {
        public enum DefaultComponentTypes
        {
            Expand
        }

        protected string Id;
        public HorizontalLayoutGroup hozLayoutGroup;
        public VerticalLayoutGroup vertLayoutGroup;
        protected List<UIPanelLineSection> sections = new List<UIPanelLineSection>();
        protected List<UIPanelLine> lines = new List<UIPanelLine>();
        protected Action<UIPanelLine> onExpand;
        protected int depth = 0;
        protected UIPanel rootPanel;
        protected UIPanelLine parentLine;
        public Dictionary<DefaultComponentTypes, UIPanelLineSection> defaultSections = new Dictionary<DefaultComponentTypes, UIPanelLineSection>();

        public virtual void Initialize(int _depth, UIPanel _rootPanel, UIPanelLine _parentLine)
        {
            depth = _depth;
            rootPanel = _rootPanel;
            parentLine = _parentLine;
            if (depth > 0)
            {
                UIPanelLineSectionText sectionText = Add<UIPanelLineSectionText>();
                sectionText.text.text = "";
                sectionText.GetComponent<LayoutElement>().preferredWidth = depth * 10;
            }
            transform.localScale = Vector3.one;

        }

        public void SetId(string _id)
        {
            Id = _id;
        }

        public virtual string GetId()
        {
            return Id;
        }

        public bool IsExpanded()
        {
            return lines.Count != 0;
        }
        public T Add<T>() where T:  UIPanelLineSection
        {
            rootPanel.MarkUpdated();
            string prefabId = typeof(T).Name;
      
            T section =
                GameManager.Instance.prefabManager.Create(prefabId, Vector3.zero, hozLayoutGroup.transform)
                    .GetComponent<T>();
            if (section == null)
            {
                throw new SystemException($"Cannot find `{prefabId}`'s component of same type");
            }
            
            section.Initialize();
            section.transform.SetAsLastSibling();
            sections.Add(section);
            return section;
        }

       

        public void CleanUp(bool setActive = false)
        {    
            foreach (UIPanelLineSection section in sections)
            {
                section.transform.SetParent(null);
                section.gameObject.SetActive(false);
            }

            sections.Clear();
            foreach (UIPanelLine line in lines)
            {
                line.CleanUp();
            }
            lines.Clear();
            defaultSections.Clear();

            gameObject.SetActive(setActive);
            if (!setActive)
            {
                transform.SetParent(null);
            }
            
        }
        public virtual T AddLine<T>()  where T:  UIPanelLine
        {
            rootPanel.MarkUpdated();
            string prefabId = typeof(T).Name;
            T panelLine =
                GameManager.Instance.prefabManager.Create(prefabId, Vector3.zero, vertLayoutGroup.transform)
                    .GetComponent<T>();
            panelLine.Initialize(depth + 1, rootPanel,  this);
            lines.Add(panelLine);
            panelLine.transform.SetAsLastSibling();
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
        public UIPanelLine GetLineById(string _id)
        {
            return lines.Find(line => line.GetId() == _id);
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
                if (IsExpanded())
                {
                    Compress();
                    return;
                } 
               Expand();
            });
            defaultSections.Add(DefaultComponentTypes.Expand, button);

        }

        public void Expand()
        {
            UIPanelLineSectionButton button =
                (defaultSections[DefaultComponentTypes.Expand] as UIPanelLineSectionButton);
            if (button == null)
            {
                throw new SystemException($"Cannot find `{DefaultComponentTypes.Expand}` component");
            }
                button.text.text = "-";
            onExpand.Invoke(this);
            rootPanel.Refresh();
        }
        public void Compress()
        {
            
            UIPanelLineSectionButton button =
                (defaultSections[DefaultComponentTypes.Expand] as UIPanelLineSectionButton);
            if (button == null)
            {
                throw new SystemException($"Cannot find `{DefaultComponentTypes.Expand}` component");
            }
            button.text.text = "+";
            ClearChildLines();
            rootPanel.Refresh();
        }

        public virtual void Refresh()
        {
            foreach (UIPanelLine line in lines)
            {
                line.Refresh();
            }

            if (vertLayoutGroup == null)
            {
                Debug.LogError($"{gameObject.name}.vertLayoutGroup is null");
                return;
            }
            RectTransform rectTransform = vertLayoutGroup.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError($"{gameObject.name}.vertLayoutGroup.GetComponent<RectTransform>() is null");
                return;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(vertLayoutGroup.GetComponent<RectTransform>());
        }

        public List<UIPanelLineSection> GetSections()
        {
            return sections;
        }
        public T GetSectionById<T>(string _id) where T : UIPanelLineSection
        {
            UIPanelLineSection section = sections.Find((section) => section.GetId() == _id);
            if (section == null)
            {
                throw new SystemException($"Cannot find `{_id}` section");
            }
            return section as T;
        }

        public void RefreshLayout()
        {
            foreach (UIPanelLineSection section in sections)
            {
                section.RefreshLayout();
            }
            foreach (UIPanelLine line in lines)
            {
                line.RefreshLayout();
            }
            if (hozLayoutGroup != null)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(hozLayoutGroup.GetComponent<RectTransform>());
            }
        }
    }

}