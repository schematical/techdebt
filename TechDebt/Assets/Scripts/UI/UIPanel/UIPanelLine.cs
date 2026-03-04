using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIPanelLine: MonoBehaviour
    {
        public HorizontalLayoutGroup layoutGroup;
        public List<UIPanelLineSection> sections = new List<UIPanelLineSection>();
        public UIPanelLineSection Add<T>(UIPanelLineSectionOptions options)
        {
            string prefabId = typeof(T).Name;
      
            UIPanelLineSection section =
                GameManager.Instance.prefabManager.Create(prefabId, Vector3.zero, layoutGroup.transform)
                    .GetComponent<T>() as UIPanelLineSection;
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
            gameObject.SetActive(false);
        }
    }

    public class UIPanelLineSectionOptions
    {
        public string text = "";
        public int fontSize = 22;
        public int width;
        public Color textColor = Color.white;
        public Sprite sprite;

        public Action onClick;
    }

   
}