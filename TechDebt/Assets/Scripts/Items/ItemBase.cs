using UnityEngine;
using UnityEngine.EventSystems; // Required for IPointerClickHandler

namespace Items
{
    public class ItemBase : MonoBehaviour, IPointerClickHandler
    {
        private bool _isTaskCreated = false; // Add this flag to prevent multiple tasks for the same item.

        public virtual void Use()
        {
            
        }

        public virtual string UseVerb()
        {
            return "Use";
        }

        private void Awake()
        {
            // Ensure a Collider2D exists for click detection by the EventSystem
            if (GetComponent<Collider2D>() == null)
            {
                var collider = gameObject.AddComponent<BoxCollider2D>();
                // You might want to adjust the collider size based on sprite bounds if applicable
                // For now, a default BoxCollider2D will work for basic clicks.
                // Ensure it's a trigger if you don't want it to block movement/physics.
                collider.isTrigger = true; 
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isTaskCreated) return; // Prevent creating multiple tasks for the same item.

            // Notify the UIManager to show the detail panel for this item.
            GameManager.Instance.UIManager.ShowItemDetail(this);
        }

        public void MarkTaskCreated()
        {
            _isTaskCreated = true;
        }
    }
}