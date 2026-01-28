// Server.cs

using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Desk : InfrastructureInstance
{
    public int lastDisplayedProgress = -1;
    public override void Initialize()
    {
        base.Initialize();
        attentionIconColor = Color.blue;
    }

    public void OnResearchProgress(Vector3 position)
    {
        if (GameManager.Instance.CurrentlyResearchingTechnology == null)
        {
            return;
        }
        int progress = (int)Math.Floor(GameManager.Instance.CurrentlyResearchingTechnology.CurrentResearchProgress /
            GameManager.Instance.CurrentlyResearchingTechnology.ResearchPointCost * 100);
        // Emit a "+1" text particle at the given position.
        if (progress % 10 == 0 && lastDisplayedProgress != progress)
        {
            GameManager.Instance.FloatingTextFactory.ShowText($"{progress}%", position, new Color(0.2f, 0.6f, 1f));//  + new Vector3(0, 1, 3));
            lastDisplayedProgress = progress;
        }
    }
    public void OnCodeProgress(Vector3 position)
    {
       
        
    }
    
    public override void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.UIManager.Close();
        GameManager.Instance.UIManager.ToggleTechTreePanel();
        //GameManager.Instance.UIManager.deskMenuPanel.gameObject.SetActive(true);
    }

    public override Vector3 GetInteractionPosition()
    {
        return transform.position + new Vector3(-0.4f, 1, 0);
    }
}