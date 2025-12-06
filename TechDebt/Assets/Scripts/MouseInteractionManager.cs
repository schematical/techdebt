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
                    UIManager.Instance.HideTooltip();
                    currentlyOpenPanelInstance = null;
                }
                // If we clicked a different instance, or no panel was open, open for the new one
                else
                {
                    if (currentlyOpenPanelInstance != null) UIManager.Instance.HideTooltip(); // Close previous
                    UIManager.Instance.ShowInfrastructureTooltip(clickedInstance);
                    currentlyOpenPanelInstance = clickedInstance;
                }
            }
            else
            {
                // If we clicked nothing, close any open panel
                if (currentlyOpenPanelInstance != null)
                {
                    UIManager.Instance.HideTooltip();
                    currentlyOpenPanelInstance = null;
                }
            }
        }
    }
}
