// FloatingTextFactory.cs
using UnityEngine;
using System.Collections.Generic;

public class FloatingTextFactory : MonoBehaviour
{
    public static FloatingTextFactory Instance { get; private set; }

    [Header("Prefab")]
    public GameObject floatingTextPrefab; 

    [Header("Pooling")]
    public int initialPoolSize = 20;
    private Queue<FloatingText> objectPool = new Queue<FloatingText>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (floatingTextPrefab == null)
        {
            Debug.LogError("FloatingTextFactory is missing the 'floatingTextPrefab'. Please assign it in the Inspector.", this);
            return;
        }
        
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewObjectForPool();
        }
    }

    private FloatingText CreateNewObjectForPool()
    {
        GameObject newObj = Instantiate(floatingTextPrefab, transform);
        FloatingText floatingText = newObj.GetComponent<FloatingText>();
        newObj.SetActive(false); // Start disabled
        objectPool.Enqueue(floatingText);
        return floatingText;
    }

    public void ShowText(string text, Vector3 position, Color? textColor = null)
    {
        if (objectPool.Count == 0)
        {
            // Optionally, grow the pool if it's empty
            CreateNewObjectForPool(); 
        }

        FloatingText textToShow = objectPool.Dequeue();
        
        // In case an object was destroyed, create a new one.
        if (textToShow == null)
        {
            textToShow = CreateNewObjectForPool();
        }
        Vector3 shake = new Vector3(Random.Range(-10, 10)/10, Random.Range(-10,10)/10, position.z);
        textToShow.Show(text, position + shake, textColor);
        
        // Re-queue the object after its lifetime is over
        StartCoroutine(RequeueAfterLifetime(textToShow));
    }

    private System.Collections.IEnumerator RequeueAfterLifetime(FloatingText textObject)
    {
        // Wait for the object to deactivate itself
        yield return new WaitUntil(() => !textObject.gameObject.activeSelf);
        objectPool.Enqueue(textObject);
    }
}
