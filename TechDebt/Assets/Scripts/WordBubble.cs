// WordBubble.cs
using TMPro;
using UnityEngine;

public class WordBubble : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    private Transform _targetToFollow;
    private Vector3 _offset = new Vector3(0, 1f, 0); // Position bubble above the target

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

        // --- Debugging for SpriteRenderer ---
        SpriteRenderer bubbleSprite = GetComponent<SpriteRenderer>();
        if (bubbleSprite != null)
        {
            Debug.Log($"[WordBubble Debug] SpriteRenderer component found. Is Enabled: {bubbleSprite.enabled}", this);
            Debug.Log($"[WordBubble Debug] SpriteRenderer Color: {bubbleSprite.color}", this);
            Debug.Log($"[WordBubble Debug] SpriteRenderer Sorting Layer: {bubbleSprite.sortingLayerName}", this);
            Debug.Log($"[WordBubble Debug] SpriteRenderer Order in Layer: {bubbleSprite.sortingOrder}", this);
        }
        else
        {
            Debug.LogWarning("[WordBubble Debug] No SpriteRenderer component found on the WordBubble prefab's root.", this);
        }

        UpdatePosition();
    }

    private void Update()
    {
        UpdatePosition();
    }
    
    /// <summary>
    /// Updates the bubble's position to follow its target in world space.
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
