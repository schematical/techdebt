using UI;
using UnityEngine;

public class UISaveSlotListPanel : UIPanel
{
    public override void Show()
    {
        base.Show();
        Refresh();
    }

    public void Refresh()
    {
        CleanUp();
        AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "Select Save Slot";

        for (int i = 0; i < 3; i++)
        {
            int slotIndex = i;
            MetaProgressData data = MetaGameManager.LoadProgress(slotIndex);
            UIPanelLine line = AddLine<UIPanelLine>();

            if (data != null)
            {
                line.Add<UIPanelLineSectionText>().text.text = $"Slot {slotIndex + 1}: {data.completedRuns} Runs";
                UIPanelLineSectionButton selectBtn = line.Add<UIPanelLineSectionButton>();
                selectBtn.text.text = "Select";
                selectBtn.button.onClick.AddListener(() => SelectSlot(slotIndex));
            }
            else
            {
                line.Add<UIPanelLineSectionText>().text.text = $"Slot {slotIndex + 1}: Empty";
                UIPanelLineSectionButton createBtn = line.Add<UIPanelLineSectionButton>();
                createBtn.text.text = "Create";
                createBtn.button.onClick.AddListener(() => SelectSlot(slotIndex));
            }
        }

        AddButton("Back", () => { Close(); GameManager.Instance.UIManager.mainMenu.Show(); });
    }

    private void SelectSlot(int index)
    {
        MetaGameManager.CurrentSlotIndex = index;
        
        // If the slot is empty, initialize it with default progress so it exists on disk
        if (MetaGameManager.LoadProgress(index) == null)
        {
            MetaGameManager.SaveProgress(new MetaProgressData());
        }

        Close();
        GameManager.Instance.UIManager.saveSlotDetailPanel.Show();
    }
}