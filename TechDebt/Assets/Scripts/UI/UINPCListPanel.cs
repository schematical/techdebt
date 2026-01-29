using UnityEngine;

namespace UI
{
    public class UINPCListPanel : UIPanel
    {
   
        public void Show()
        {
            Refresh();
        }
        public void Refresh()
        {
            gameObject.SetActive(true);
            for (int i = scrollContent.transform.childCount - 1; i > 0; i--)
            {
                Destroy(scrollContent.transform.GetChild(i).gameObject);
            }

            foreach (NPCBase npc in GameManager.Instance.AllNpcs)
            {
                AddButton(npc.name, () => GameManager.Instance.UIManager.npcDetailPanel.Show(npc));
            }
        }

    }
}