using UI;
using UnityEngine;

public class UISaveSlotListPanel : UIMultiSelectPanel
{
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
                    SelectSlot(int.Parse(id));
                }
            });
        }
    }

    private void SelectSlot(int index)
    {
        MetaGameManager.CurrentSlotIndex = index;

        // If the slot is empty, initialize it with default progress so it exists on disk
        if (MetaGameManager.LoadProgressFromSaveSlot(index) == null)
        {
            MetaGameManager.SaveProgress(new MetaProgressData());
        }

        Close();
        GameManager.Instance.UIManager.saveSlotDetailPanel.Show();
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
