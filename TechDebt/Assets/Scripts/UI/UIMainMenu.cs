using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UI;
using UnityEditor;
using UnityEngine.Serialization;
using UnityEngine.UnityConsent;

public class UIMainMenu : UIPanel
{
    public override void Show()
    {
        base.Show();
        GameManager.Instance.UIManager.Block();
        
        AddButton("Play", () => { Close(); GameManager.Instance.UIManager.saveSlotListPanel.Show(); });
        AddButton("Discord", OpenDiscord);  
        AddButton("Feedback", () => Application.OpenURL("https://forms.gle/NRRbLNhtoaJQrRzp9"));     
        AddButton("Wishlist now!", () => Application.OpenURL("https://store.steampowered.com/app/4567430/Tech_Debt/"));     
        AddButton("Quit", () => { Application.Quit(); });
        AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Play Test - v{Application.version}";
    }

    private void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/yFDKNDBquZ");
    }
    
}