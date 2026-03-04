using System.Collections.Generic;
using Stats;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UIStatCollectionListPanel: MonoBehaviour
    {
        protected StatsCollection  statCollection;
        public TextMeshProUGUI titleText;
        public GameObject titleObject;
        protected List<UIStatDataDetailPanel> detailPanels = new List<UIStatDataDetailPanel>();
        public void Initialize(StatsCollection _statCollection, string title = null)
        {
            CleanUp();
            titleObject.SetActive(false);
            statCollection = _statCollection;
            foreach (StatData statData in statCollection.Stats.Values)
            {
                UIStatDataDetailPanel detailPanel =
                    GameManager.Instance.prefabManager.Create("UIStatDataDetailPanel", Vector3.zero, transform)
                        .GetComponent<UIStatDataDetailPanel>();
                detailPanel.Initialize(statData);
                detailPanels.Add(detailPanel);
            }

            if (title != null)
            {
                SetTitle(title);
            }

        }

        void CleanUp()
        {
            foreach (UIStatDataDetailPanel panel in detailPanels)
            {
                panel.gameObject.SetActive(false);
            }
            detailPanels.Clear();
        }

        public void SetTitle(string title)
        {
            titleObject.SetActive(true);
            titleText.text = title;
        }
    }
    
   
    
}