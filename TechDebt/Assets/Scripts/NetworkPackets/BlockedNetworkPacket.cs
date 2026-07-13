using System;
using Infrastructure;
using Tutorial;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;


namespace DefaultNamespace.NetworkPackets
{
    public class BlockedNetworkPacket: NetworkPacket
    {
 
        public override void Initialize(NetworkPacketData npData, string fileName, int size,
            InfrastructureInstance origin = null)
        {
            base.Initialize(npData, fileName, size, origin);
            spriteRenderer.sprite = GameManager.Instance.SpriteManager.GetSprite("SQLInjectionNetworkPacket", "0");
        }

        public override void StartReturn()
        {
            spriteRenderer.flipX = true;
            base.StartReturn();
        }

        protected override void FixedUpdate()
        {
            if (!IsReturning())
            {
                 base.FixedUpdate();
                 return;
            }

            BlockTimer -= Time.fixedDeltaTime;
            if (BlockTimer <= 0)
            {
                StartReturn();
                MoveToNextNode();
            }
            base.FixedUpdate();
        }
        /*public override void MarkStolen()
        {
            spriteRenderer.sprite = GameManager.Instance.SpriteManager.GetSprite("SQLInjectionNetworkPacket", "1");
            StartReturn();
            MoveToNextNode();
        }*/
        /*public override NetworkPacketRouteAction OnInfraContact(InfrastructureInstance infrastructureInstance)
        {
            if (IsReturning())
            {
                return NetworkPacketRouteAction.Normal;
            }
            switch (infrastructureInstance.data.worldObjectType)
            {
                case(WorldObjectType.Type.ApplicationServer):
                    float inputValidation = GameManager.Instance.Stats.GetStatValue(StatType.Infra_InputValidation);
                    if (Random.value > inputValidation)
                    {
                        if (GameManager.Instance.GetInfrastructureInstanceByID("dedicated-db").IsActive())
                        {
                            return NetworkPacketRouteAction.Normal;
                        }
                        // Steal PPI from the application layer
                        MarkStolen();
                        return NetworkPacketRouteAction.DefferToPacket;
                    }
                    GameManager.Instance.FloatingTextFactory.ShowText($"Input Validation Block SQL Injection", transform.position - new Vector3(0,1), Color.purple);
                    StartReturn();
                    // TODO: Frowny face packet.
                    
                    return NetworkPacketRouteAction.DefferToPacket;
                case(WorldObjectType.Type.DedicatedDB):
                    MarkStolen();
                    return NetworkPacketRouteAction.Normal;
                default:
                    Debug.LogError($"SQLInjectionNetworkPacket is hitting {infrastructureInstance.data.Id}");
                    return NetworkPacketRouteAction.Normal;
                    
            }
      
        }*/
        /*public override void OnLeftClick(PointerEventData eventData)
        {
            base.OnLeftClick(eventData);
            if (GameManager.Instance.TutorialManager != null)
            {
                GameManager.Instance.TutorialManager.ForceRender(TutorialStepId.NPC_SQLInjection_View);
            }
        }*/
       
    }
}