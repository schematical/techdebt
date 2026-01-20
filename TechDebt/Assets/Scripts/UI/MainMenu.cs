using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UI;
using UnityEngine.Serialization;

public class MainMenu : MonoBehaviour
{

    public Button newGameBtn;
    [FormerlySerializedAs("loadGameBtn")] public Button unlockBtn;
    public Button challengesBtn;

    public Button settingsBtn;

    private void Start()
    {
        Debug.Log("MainMenu script started. Time scale is now 1.");
        Time.timeScale = 1f;
        
        
        newGameBtn.onClick.AddListener(NewGame);
    

        unlockBtn.onClick.AddListener(ShowUnlockPanel);
        challengesBtn.onClick.AddListener(ShowChallenges);
     

        settingsBtn.onClick.AddListener(OpenSettings);
        
    }

  

    public void NewGame()
    {
        SceneManager.LoadScene("GameScene");
    }
    

    public void ShowUnlockPanel()
    {

        UIMainMenuCanvas.Instance.ClosePanels();
        UIMainMenuCanvas.Instance.metaUnlockPanel.gameObject.SetActive(true);
    }  
    public void ShowChallenges()
    {

  
        UIMainMenuCanvas.Instance.ClosePanels();
        UIMainMenuCanvas.Instance.uiMetaChallengesPanel.gameObject.SetActive(true);
    }

    public void OpenSettings()
    {
        Debug.Log("OpenSettings button clicked!");
        // Open settings menu
    }

   
}
