// WordBubble.cs
using TMPro;
using UnityEngine;

public class WordBubble : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    private Transform _targetToFollow;
    private Vector3 _offset = new Vector3(0, 1.2f, 0); // Position bubble above the target

    /// <summary>
    /// Initializes the word bubble with a message and a target to follow.
    /// </summary>
    /// <param name="message">The text to display in the bubble.</param>
    /// <param name="target">The transform the bubble should follow.</param>
    public void Setup(string message, Transform target)
    {
        if (textMesh == null)
        {
            textMesh = GetComponentInChildren<TextMeshProUGUI>();
            if (textMesh == null)
            {
                Debug.LogError("WordBubble requires a TextMeshProUGUI component in its children.", this);
                Destroy(gameObject);
                return;
            }
        }
        
        textMesh.text = message;
        _targetToFollow = target;
        UpdatePosition();

        // --- Debugging Logs ---
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"[WordBubble Debug] Canvas Render Mode: {canvas.renderMode}", this);
            Debug.Log($"[WordBubble Debug] Canvas Sorting Layer: {canvas.sortingLayerName}", this);
            Debug.Log($"[WordBubble Debug] Canvas Sorting Order: {canvas.sortingOrder}", this);
        }
        else
        {
            Debug.LogWarning("[WordBubble Debug] No Canvas found in parents. This is likely the problem. It needs a Canvas to render.", this);
        }

        Debug.Log($"[WordBubble Debug] Text Content: '{textMesh.text}'", this);
        Debug.Log($"[WordBubble Debug] Text Color: {textMesh.color}", this);
        Debug.Log($"[WordBubble Debug] Is TextMeshPro Active and Enabled: {textMesh.isActiveAndEnabled}", this);
        Debug.Log($"[WordBubble Debug] Transform Scale: {transform.localScale}", this);
        Debug.Log($"[WordBubble Debug] GameObject Active: {gameObject.activeInHierarchy}", this);
    }

    private void Update()
    {
        UpdatePosition();
    }
    
    /// <summary>
    /// Updates the bubble's position to follow its target.
    /// </summary>
    private void UpdatePosition()
    {
        if (_targetToFollow != null)
        {
            transform.position = _targetToFollow.position + _offset;
        }
        else
        {
            // If the target is destroyed, the bubble should also disappear.
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Destroys the word bubble GameObject.
    /// </summary>
    public void Close()
    {
        Destroy(gameObject);
    }
}
