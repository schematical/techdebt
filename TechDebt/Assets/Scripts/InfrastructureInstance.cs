// InfrastructureInstance.cs
using UnityEngine;

public class InfrastructureInstance : MonoBehaviour
{
    public InfrastructureData data;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Initialize(InfrastructureData infraData)
    {
        this.data = infraData;
        UpdateAppearance();
    }

    public void SetState(InfrastructureData.State newState)
    {
        data.CurrentState = newState;
        UpdateAppearance();
    }

    public void UpdateAppearance()
    {

        switch (data.CurrentState)
        {
            case InfrastructureData.State.Locked:
                // Ghosted / Outlined appearance
                spriteRenderer.color = new Color(0.5f, 0.5f, 1f, 0.3f); 
                break;
            case InfrastructureData.State.Unlocked:
                // Available to be planned
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.7f);
                break;
            case InfrastructureData.State.Planned:
                // Construction appearance
                spriteRenderer.color = new Color(1f, 0.8f, 0.3f, 0.7f); 
                break;
            case InfrastructureData.State.Operational:
                // Normal appearance
                spriteRenderer.color = Color.white; 
                break;
        }
Debug.Log("Updating Appearence: " + spriteRenderer.color);
    }
}
