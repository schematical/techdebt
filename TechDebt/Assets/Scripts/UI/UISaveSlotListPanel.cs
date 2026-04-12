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
                line.Add<UIPanelLineSectionText>().text.text = $"Slot {slotIndex + 1}: {data.completedRuns} Runs, {data.successfulExits} Wins";
                UIPanelLineSectionButton selectBtn = line.Add<UIPanelLineSectionButton>();
                selectBtn.text.text = "Select";
                selectBtn.button.onClick.AddListener(() => SelectSlot(slotIndex));

                UIPanelLineSectionButton deleteBtn = line.Add<UIPanelLineSectionButton>();
                deleteBtn.text.text = "Delete";
                deleteBtn.button.onClick.AddListener(() => DeleteSlot(slotIndex));
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
        Close();
        GameManager.Instance.UIManager.saveSlotDetailPanel.Show();
    }

    private void DeleteSlot(int index)
    {
        GameManager.Instance.UIManager.dialogPanel.ShowDialog(
            null,
            $"Are you sure you want to delete slot {index + 1}? This cannot be undone.",
            new System.Collections.Generic.List<DialogButtonOption>
            {
                new DialogButtonOption { Text = "Delete", OnClick = () => { MetaGameManager.DeleteSlot(index); Refresh(); } },
                new DialogButtonOption { Text = "Cancel", OnClick = () => { } }
            }
        );
    }
}