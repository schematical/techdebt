using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UI;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPanel : UIGameObject
{
    public bool runUICloseOnShow = true;
    public TextMeshProUGUI titleText; // Assign the TextMeshProUGUI component for the panel title
    public Transform scrollContent;   // Assign the Transform for the scrollable content area
    public Button closeButton; // Assign the Button component for the panel's close button
    protected List<UIPanelLine> lines = new List<UIPanelLine>();
    protected override void Awake()
    {
        if (closeButton != null)
        {
           
            closeButton.onClick.AddListener(() => Close());
        }
        else
        {
            // Debug.LogError($"{gameObject.name} is missing `closeButton`");
        }
        base.Awake();
    }
    
    public override void Show()
    {
        if (runUICloseOnShow)
        {
            GameManager.Instance.UIManager.CloseSideBars();
        }
        base.Show();
        
    }

    public virtual UIPanelButton AddButton(string buttonText, UnityAction onClickAction)
    {
        UIPanelButton button = AddLine<UIPanelButton>();
        button.text.text = buttonText;
        button.button.onClick.RemoveAllListeners();
        button.button.onClick.AddListener(onClickAction);
        button.transform.SetAsLastSibling();
        return button;
    }

    public override void Close(bool forceClose = false)
    {
        CleanUp();
        base.Close(forceClose);
    }

    public void CleanUp()
    {
        foreach (UIPanelLine line in lines)
        {
            line.CleanUp();
        }
        lines.Clear();
    }

    public T AddLine<T>() where T:  UIPanelLine
    {
        string prefabId = typeof(T).Name;
        T panelLine =
            GameManager.Instance.prefabManager.Create(prefabId, Vector3.zero, scrollContent.transform)
                .GetComponent<T>();
        if (panelLine == null)
        {
            throw new SystemException($"Some how {prefabId} is null");
        }
        lines.Add(panelLine);
        panelLine.Initialize(0, this, null);
        panelLine.transform.SetAsLastSibling();
        return panelLine;
    }

    public void Refresh()
    {
        foreach (UIPanelLine line in lines)
        {
            line.Refresh();
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollContent.GetComponent<RectTransform>());
    }
    public UIPanelLine GetLineById(string _id)
    {
        return lines.Find(line => line.GetId() == _id);
    }
}