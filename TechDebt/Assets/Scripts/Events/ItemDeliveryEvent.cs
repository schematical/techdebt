using System;
using UnityEngine;

namespace Events
{
    public class ItemDeliveryEvent: EventBase
    {
        public override void Apply()
        {

            InfrastructureInstance door = GameManager.Instance.GetInfrastructureInstanceByID("door");
            if (door == null)
            {
                Debug.LogError("Cannot spawn DeliveryNPC because 'door' infrastructure was not found.");
                return;
            }

            GameObject npcGO = GameManager.Instance.prefabManager.Create("DeliveryNPC", door.transform.position);
            if (npcGO == null)
            {
                Debug.LogError("Failed to create 'DeliveryNPC' from PrefabManager. Is the prefab configured?");
                return;
            }

            DeliveryNPC deliveryNpc = npcGO.GetComponent<DeliveryNPC>();
            if (deliveryNpc != null)
            {
                deliveryNpc.Initialize(door.transform.position);
                DeliverItemTask deliveryTask = new DeliverItemTask();
                deliveryNpc.AssignTask(deliveryTask);
            }
            else
            {
                Debug.LogError("'DeliveryNPC' prefab is missing the DeliveryNPC component.");
            }
        }

    }
}