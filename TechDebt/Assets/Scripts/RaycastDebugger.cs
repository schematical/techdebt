// RaycastDebugger.cs
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // ADD THIS

public class RaycastDebugger : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame) // USE NEW INPUT SYSTEM
        {
            // Create a pointer event data to get all raycast results
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Mouse.current.position.ReadValue(); // USE NEW INPUT SYSTEM
            
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            
            if (results.Count > 0)
            {
                Debug.Log("--- Raycast Hit ---");
                foreach (var result in results)
                {
                    Debug.Log($"Hit: {result.gameObject.name}, Layer: {LayerMask.LayerToName(result.gameObject.layer)}, Distance: {result.distance}");
                }
                Debug.Log("---------------------");
            }
            else
            {
                Debug.Log("--- Raycast Hit Nothing ---");
            }
        }
    }
}
