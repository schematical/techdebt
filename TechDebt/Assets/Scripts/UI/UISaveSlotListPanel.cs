using UI;
using UnityEngine;

public class UISaveSlotListPanel : UIMultiSelectPanel
{
    public UIButton backButton;

    protected override void Awake()
    {
        base.Awake();
        backButton.button.onClick.AddListener(OnBack);
    }
    public override void Show()
    {
        Display("Select Save Slot", "Select a profile to continue");
        Refresh();
    }

    public void Refresh()
    {
        for (int i = 0; i < 3; i++)
        {
            int slotIndex = i;
            MetaProgressData data = MetaGameManager.LoadProgressFromSaveSlot(slotIndex);

            string primaryText = $"Slot {slotIndex + 1}";
            string secondaryText = data != null ? $"{data.completedRuns} Runs Completed" : "Empty Slot";
            
            Sprite icon = GameManager.Instance.SpriteManager.GetSprite("IconSave"); 

            UIMultiSelectOption option = Add(slotIndex.ToString(), icon, primaryText, secondaryText);
            option.OnInteract((type, id) =>
            {
                if (type == UIMultiSelectOption.InteractionType.Select)
                {
                    
                    int index = int.Parse(id);
                    
                    MetaGameManager.SetCurrentSaveSlot(index); 

                    if (data == null)
                    {
                        MetaGameManager.SaveProgress(new MetaProgressData());
                    }

                    Close();
                    GameManager.Instance.UIManager.saveSlotDetailPanel.Show();
                }
            });
        }
    }

    public void OnBack()
    {
        Close();
        GameManager.Instance.UIManager.mainMenu.Show();
    }

    
    /*public override void Close(bool forceClose = false)
    {
        base.Close(forceClose);
        
        // If we've closed the panel and haven't selected a slot yet (and game hasn't started),
        // we should probably return to the main menu.
        if (GameManager.Instance.UIManager.saveSlotDetailPanel.GetPanelState() == UIState.Closed)
        {
            GameManager.Instance.UIManager.mainMenu.Show();
        }
    }*/
}
