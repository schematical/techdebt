using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UI;
using UnityEngine.Serialization;

public class MainMenu : MonoBehaviour
{
    public TextMeshProUGUI metaCurrencyText;
    public Button newGameBtn;
    [FormerlySerializedAs("loadGameBtn")] public Button unlockBtn;
    public Button settingsBtn;

    private void Start()
    {
        Debug.Log("MainMenu script started. Time scale is now 1.");
        Time.timeScale = 1f;
        
        UpdateMetaCurrencyText();
        newGameBtn.onClick.AddListener(NewGame);
    

        unlockBtn.onClick.AddListener(ShowUnlockPanel);
     

        settingsBtn.onClick.AddListener(OpenSettings);
        
    }

  

    public void NewGame()
    {
        Debug.Log("NewGame button clicked!");
        SceneManager.LoadScene("SampleScene");
    }
    

    public void ShowUnlockPanel()
    {
        Debug.Log("LoadGame button clicked!");
        // Load game data
        UIMainMenuCanvas.Instance.ClosePanels();
        UIMainMenuCanvas.Instance.metaUnlockPanel.gameObject.SetActive(true);
    }

    public void OpenSettings()
    {
        Debug.Log("OpenSettings button clicked!");
        // Open settings menu
    }

    private void UpdateMetaCurrencyText()
    {
        if (metaCurrencyText != null)
        {
            int metaDollars = PlayerPrefs.GetInt("metaDollars", 0);
            metaCurrencyText.text = $"Schemata-Bucks: {metaDollars}";
        }
        else
        {
            Debug.LogError("metaCurrencyText is not assigned in the Inspector on the MainMenu script!");
        }
    }
}
