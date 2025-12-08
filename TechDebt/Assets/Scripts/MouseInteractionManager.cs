// MouseInteractionManager.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MouseInteractionManager : MonoBehaviour
{
    private InfrastructureInstance currentlyOpenPanelInstance;

    void Update()
    {
        // Check for a left-mouse click
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Perform a raycast to see what was clicked
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Mouse.current.position.ReadValue();

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, results);

            // If any of the raycast results hit a UI element, do not proceed with game world interactions.
            // The default UI layer is 5.
            if (results.Exists(result => result.gameObject.layer == 5))
            {
                return;
            }

            InfrastructureInstance clickedInstance = null;
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.TryGetComponent<InfrastructureInstance>(out var instance))
                {
                    clickedInstance = instance;
                    break;
                }
            }

            if (clickedInstance != null)
            {
                // If we clicked the same instance, toggle it off
                if (currentlyOpenPanelInstance == clickedInstance)
                {
                    GameManager.Instance.UIManager.HideTooltip();
                    currentlyOpenPanelInstance = null;
                }
                // If we clicked a different instance, or no panel was open, open for the new one
                else
                {
                    if (currentlyOpenPanelInstance != null) GameManager.Instance.UIManager.HideTooltip(); // Close previous
                    GameManager.Instance.UIManager.ShowInfrastructureTooltip(clickedInstance);
                    currentlyOpenPanelInstance = clickedInstance;
                }
            }
            else
            {
                // If we clicked on something that wasn't an infrastructure instance,
                // and it wasn't a UI element, then close any open panel.
                if (!EventSystem.current.IsPointerOverGameObject() && currentlyOpenPanelInstance != null)
                {
                    GameManager.Instance.UIManager.HideTooltip();
                    currentlyOpenPanelInstance = null;
                }
            }
        }
    }
}
