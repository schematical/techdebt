using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items
{
    public class BoxItem : ItemBase
    {
        public override string UseVerb()
        {
            return "Open";
        }

        public override void Use()
        {
            // Get a list of all possible items to spawn, excluding the box itself.
            List<ItemData> possibleItems = GameManager.Instance.Items.Where(item => item.Id != "BoxItem").ToList();

            if (possibleItems.Count == 0)
            {
                Debug.LogError("No items (excluding BoxItem) are defined in GameManager.Items to spawn.");
                Destroy(gameObject); // Destroy the box anyway to prevent it from being stuck.
                return;
            }

            // Calculate total probability for weighted random selection
            int totalProbability = possibleItems.Sum(item => item.Probability);
            int randomValue = Random.Range(0, totalProbability);

            ItemData selectedItem = null;
            int cumulativeProbability = 0;

            foreach (var item in possibleItems)
            {
                cumulativeProbability += item.Probability;
                if (randomValue < cumulativeProbability)
                {
                    selectedItem = item;
                    break;
                }
            }
            
            // Fallback in case something goes wrong with the probability calculation
            if (selectedItem == null)
            {
                Debug.LogError($"No item selected. cumulativeProbability: {cumulativeProbability} - randomValue: {randomValue}");
                
            }
            else
            {

                // Use the PrefabManager to create the selected item at the box's position
                GameManager.Instance.prefabManager.Create(selectedItem.Id, transform.position);
            }

            // Deactivate the box instead of destroying it for pooling purposes
            gameObject.SetActive(false);
        }
    }
}
