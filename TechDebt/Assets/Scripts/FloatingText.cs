// FloatingText.cs
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Animation Settings")]
    public float lifetime = 1.5f;
    public Vector3 moveSpeed = new Vector3(0, 0.75f, 0); // Moves upwards
    public float fadeOutTime = 0.5f;

    private TextMeshProUGUI textMesh;
    private float timeElapsed = 0f;
    private Color defaultColor = Color.white;

    private void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        if (textMesh == null)
        {
            Debug.LogError("FloatingText requires a TextMeshProUGUI component in its children.", this);
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (timeElapsed < lifetime)
        {
            // Move
            transform.position += moveSpeed * Time.deltaTime;

            // Fade Out
            if (timeElapsed > lifetime - fadeOutTime)
            {
                float fadeProgress = (timeElapsed - (lifetime - fadeOutTime)) / fadeOutTime;
                Color newColor = textMesh.color;
                newColor.a = Mathf.Lerp(defaultColor.a, 0f, fadeProgress);
                textMesh.color = newColor;
            }

            timeElapsed += Time.deltaTime;
        }
        else
        {
            // Lifetime is over, reset or destroy
            gameObject.SetActive(false); 
        }
    }

    public void Show(string text, Vector3 position, Color? textColor = null)
    {
        transform.position = position;
        textMesh.text = text;
        defaultColor = textColor ?? Color.white;
        textMesh.color = defaultColor;
        timeElapsed = 0f;
        gameObject.SetActive(true);
    }
}
