using System;
using System.Collections.Generic;
using Infrastructure;
using NPCs;
using Stats;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class UIGlobalStatsPanel: UIPanel
    {
        public enum LineType
        {
            Stats,
            NetworkPacketData
        }
        protected UIStatCollectionPanelLine statCollectionPanelLine;
        protected UIStatCollectionPanelLine networkLine;
        protected UIPanelLine worldObjectTypesLine;
        
        void Start()
        {
           
        }
        // ReSharper disable Unity.PerformanceAnalysis
        public override void Show()
        {

            if (!IsOpen())
            {
                base.Show();
            }
           
            CleanUp();

            statCollectionPanelLine = AddLine<UIStatCollectionPanelLine>();
            statCollectionPanelLine.SetStatCollection(GameManager.Instance.Stats);
            networkLine = AddLine<UIStatCollectionPanelLine>();
            networkLine.Add<UIPanelLineSectionText>().text.text = "Network Packet Data:";
            networkLine.SetExpandable((line =>
            {
                
                foreach (NetworkPacketData networkPacketData in GameManager.Instance.GetNetworkPacketDatas())
                {
                    UIStatCollectionPanelLine networkDetailLine = line.AddLine<UIStatCollectionPanelLine>();
                    networkDetailLine.SetStatCollection( 
                        networkPacketData.Stats,
                        networkPacketData.Type.ToString()
                    );
                    networkDetailLine.SetId(networkPacketData.Type.ToString());
                }
            }));   
            worldObjectTypesLine = AddLine<UIPanelLine>();
            worldObjectTypesLine.Add<UIPanelLineSectionText>().text.text = "Infrastructure Types:";
            worldObjectTypesLine.SetExpandable((worldObjectTypesLine =>
            {
                foreach (WorldObjectType worldObjectType in GameManager.Instance.WorldObjectTypes.Values)
                {
                    if (!worldObjectType.DisplayInGlobalUI())
                    {
                        continue;
                    }
                    UIPanelLine worldObjectTypeLine = worldObjectTypesLine.AddLine<UIStatCollectionPanelLine>();
                    worldObjectTypeLine.Add<UIPanelLineSectionText>().text.text = worldObjectType.DisplayName;
                    worldObjectTypeLine.SetId(worldObjectType.type.ToString());
                    worldObjectTypeLine.SetExpandable((worldObjectTypeLine =>
                    {


                        UIStatCollectionPanelLine worldObjectTypeStatLine =
                            worldObjectTypeLine.AddLine<UIStatCollectionPanelLine>();
                        worldObjectTypeStatLine.SetStatCollection(
                            worldObjectType.Stats,
                            "Stats"
                        );
                        worldObjectTypeStatLine.SetId(LineType.Stats.ToString());

                        
                        
                        
                        UIPanelLine worldObjectTypeNetworkPacketsLine = worldObjectTypeLine.AddLine<UIPanelLine>();
                        worldObjectTypeNetworkPacketsLine.Add<UIPanelLineSectionText>().text.text = "Network Packet Data:";
                        worldObjectTypeNetworkPacketsLine.SetId(LineType.NetworkPacketData.ToString());
                        worldObjectTypeNetworkPacketsLine.SetExpandable((worldObjectTypeNetworkPacketLine =>
                        {
                            
                            foreach (InfrastructureDataNetworkPacket networkPacketData in worldObjectType.networkPackets)
                            {
                                UIStatCollectionPanelLine worldObjectTypeStatLine =
                                    worldObjectTypeNetworkPacketLine.AddLine<UIStatCollectionPanelLine>();
                                worldObjectTypeStatLine.SetStatCollection(
                                    networkPacketData.Stats,
                                    networkPacketData.PacketType.ToString()
                                );
                                worldObjectTypeStatLine.SetId(networkPacketData.PacketType.ToString());
                            }
                        }));
                        
                        
                        
                    }));

                   

                }
            }));
        
            
            
            
            UIStatCollectionPanelLine globalModifiersLine = AddLine<UIStatCollectionPanelLine>();
            globalModifiersLine.Add<UIPanelLineSectionText>().text.text = "Upgrades:";
            globalModifiersLine.SetExpandable((_globalModifiersLine =>
            {
                GameManager.Instance.Modifiers.Modifiers.Add(new ModifierBase()
                {
                    Group = ModifierBase.ModifierGroup.Release,
                    Target = ModifierBase.ModifierTarget.Run,
                    Type = ModifierBase.ModifierType.Run_Stat,
                    Id = "input_validation",
                    Name = "Input Validation",
                    StatType = StatType.Infra_InputValidation,
                    // BaseValue = 1.05f,
                    IconSpriteId = "IconCode"
                });
                GameManager.Instance.Modifiers.Modifiers[0].Apply();
                GameManager.Instance.Modifiers.Modifiers[0].LevelUp(Rarity.Common);
                GameManager.Instance.Modifiers.Render(_globalModifiersLine);
            }));



        }

        public void Preview(ModifierBase modifierBase)
        {
            
           Show();
           switch (modifierBase.Type)
           {
               case (ModifierBase.ModifierType.Run_Stat):
               case (ModifierBase.ModifierType.Run_Stat_Flat):

                   statCollectionPanelLine.Preview(modifierBase);
                   break;
               case (ModifierBase.ModifierType.Global_NetworkPacketStat):
                   if (!networkLine.IsExpanded())
                   {
                       networkLine.Expand();
                   }
                   foreach (UIPanelLine line in networkLine.GetLines())
                   {
                       Debug.Log($"{line.GetId()} == {modifierBase.NetworkPacketType.ToString()} => {line.GetId() == modifierBase.NetworkPacketType.ToString()}");
                       if (line.GetId() == modifierBase.NetworkPacketType.ToString())
                       {
                           (line as UIStatCollectionPanelLine).Preview(modifierBase);
                       } /*
                       else
                       {
                           (line as UIStatCollectionPanelLine).ResetText();
                       }*/
                   }

                   break;
               case(ModifierBase.ModifierType.Infra_NetworkPacketStat):
                   
                   if (!worldObjectTypesLine.IsExpanded())
                   {
                       worldObjectTypesLine.Expand();
                   }
                   UIPanelLine worldObjectTypeLine = worldObjectTypesLine.GetLineById(modifierBase.WorldObjectType.ToString());
                   if (worldObjectTypeLine == null)
                   {
                        Debug.LogWarning($"Could not find {modifierBase.WorldObjectType.ToString()} in {worldObjectTypesLine.GetLines().Count}");
                        return;
                   }
                   if (!worldObjectTypeLine.IsExpanded())
                   {
                       worldObjectTypeLine.Expand();
                   }
                   UIPanelLine worldObjectTypeNetworkPacketDataLine = worldObjectTypeLine.GetLineById(LineType.NetworkPacketData.ToString());
                   if (worldObjectTypeNetworkPacketDataLine == null)
                   {
                       throw new SystemException($"Could not find {LineType.NetworkPacketData.ToString()} in {worldObjectTypeLine.GetLines().Count}");   
                   }
                   if (!worldObjectTypeNetworkPacketDataLine.IsExpanded())
                   {
                       worldObjectTypeNetworkPacketDataLine.Expand();
                   }
                   UIPanelLine worldObjectTypeNetworkPacketDataStatsLine = worldObjectTypeNetworkPacketDataLine.GetLineById(modifierBase.NetworkPacketType.ToString());
                   if (worldObjectTypeNetworkPacketDataStatsLine == null)
                   {
                       throw new SystemException($"Could not find {modifierBase.NetworkPacketType.ToString()} in {worldObjectTypeNetworkPacketDataLine.GetLines().Count}");   
                   }
                   if (!worldObjectTypeNetworkPacketDataStatsLine.IsExpanded())
                   {
                       worldObjectTypeNetworkPacketDataStatsLine.Expand();
                   }
                   (worldObjectTypeNetworkPacketDataStatsLine as UIStatCollectionPanelLine).Preview(modifierBase);
                   break;
               default:
                   throw new NotImplementedException();
           }
        }
    }
}