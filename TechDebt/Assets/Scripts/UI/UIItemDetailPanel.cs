using Items;
using TMPro;
using UnityEngine.UI;

namespace UI
{
    public class UIItemDetailPanel: UIPanel
    {
        public TextMeshProUGUI useButtonText;
        public Button useButton;
        public ItemBase item;
        public void Start()
        {
            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(OnUseClick);
        }

        private void OnUseClick()
        {
            GameManager.Instance.CreateUseItemTask(item);
            Close();
        }

        public void Show(ItemBase _item)
        {
            item = _item;
            useButtonText.text = item.UseVerb();
        }
    }
}