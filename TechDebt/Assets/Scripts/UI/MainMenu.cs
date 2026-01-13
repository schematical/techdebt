
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TextMeshProUGUI metaCurrencyText;

    private void Start()
    {
        UpdateMetaCurrencyText();
    }

    public void NewGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void LoadGame()
    {
        // Load game data
    }

    public void OpenSettings()
    {
        // Open settings menu
    }

    private void UpdateMetaCurrencyText()
    {
        int metaDollars = PlayerPrefs.GetInt("metaDollars", 0);
        metaCurrencyText.text = $"Schemata-Bucks: {metaDollars}";
    }
}
