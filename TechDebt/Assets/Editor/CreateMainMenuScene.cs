
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.UI;

public class CreateMainMenuScene
{
    [MenuItem("Tools/Create Main Menu Scene")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Cannot create the main menu scene while in Play Mode. Please exit Play Mode first.");
            return;
        }

        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // --- Canvas ---
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // --- Event System ---
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<InputSystemUIInputModule>();
        }

        // --- MainMenu Script ---
        GameObject mainMenuGO = new GameObject("MainMenu");
        mainMenuGO.transform.SetParent(canvasGO.transform);
        MainMenu mainMenu = mainMenuGO.AddComponent<MainMenu>();
        
        // --- Title ---
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(canvasGO.transform);
        TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = "Tech Debt: The Game";
        titleText.fontSize = 48;
        titleText.alignment = TextAlignmentOptions.Center;
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0, 150);
        titleRect.sizeDelta = new Vector2(600, 100);

        // --- Meta Currency ---
        GameObject metaCurrencyGO = new GameObject("MetaCurrencyText");
        metaCurrencyGO.transform.SetParent(canvasGO.transform);
        TextMeshProUGUI metaCurrencyText = metaCurrencyGO.AddComponent<TextMeshProUGUI>();
        metaCurrencyText.text = "Schemata-Bucks: 0";
        metaCurrencyText.fontSize = 24;
        metaCurrencyText.alignment = TextAlignmentOptions.Center;
        RectTransform metaCurrencyRect = metaCurrencyGO.GetComponent<RectTransform>();
        metaCurrencyRect.anchorMin = new Vector2(0.5f, 1f);
        metaCurrencyRect.anchorMax = new Vector2(0.5f, 1f);
        metaCurrencyRect.pivot = new Vector2(0.5f, 1f);
        metaCurrencyRect.anchoredPosition = new Vector2(0, -20);
        metaCurrencyRect.sizeDelta = new Vector2(400, 50);
        mainMenu.metaCurrencyText = metaCurrencyText;

        // --- Buttons ---
        CreateButton(canvasGO.transform, "New Game Button", "New Game", new Vector2(0, 50), mainMenu.NewGame);
        CreateButton(canvasGO.transform, "Load Game Button", "Load Game", new Vector2(0, 0), mainMenu.ShowUnlockPanel);
        CreateButton(canvasGO.transform, "Settings Button", "Settings", new Vector2(0, -50), mainMenu.OpenSettings);

        EditorSceneManager.SaveScene(newScene, "Assets/Scenes/MainMenu.unity");
    }

    private static void CreateButton(Transform parent, string name, string text, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent);
        buttonGO.AddComponent<Image>();
        Button button = buttonGO.AddComponent<Button>();
        button.onClick.AddListener(action);

        RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(200, 40);

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform);
        TextMeshProUGUI buttonText = textGO.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.color = Color.black;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.raycastTarget = false; // Add this line

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }
}
