using System;
using System.Collections.Generic;
using DefaultNamespace.Rewards;
using Infrastructure;
using NPCs;
using Rewards;
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
                        Util.GetDisplayable(
                            networkPacketData.Type.ToString()
                        )
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
                GameManager.Instance.Rewards.Render(_globalModifiersLine);
            }));
            UIStatCollectionPanelLine banishesLine = AddLine<UIStatCollectionPanelLine>();
            banishesLine.Add<UIPanelLineSectionText>().text.text = "Banishes:";
            banishesLine.SetExpandable((line =>
            {
                foreach (string mapBanishedRewardId in GameManager.Instance.Map.BanishedRewardIds)
                {
                    line.AddLine<UIStatCollectionPanelLine>().Add<UIPanelLineSectionText>().text.text =
                        mapBanishedRewardId;
                }
            }));


        }

        public void Preview(RewardBase modifierBase)
        {

            Show();
            if (modifierBase is GlobalStatModifierReward)
            {


                statCollectionPanelLine.Preview((GlobalStatModifierReward)modifierBase);
            }
            else if (modifierBase is GlobalNetworkPacketStatModifierReward)
            {
                GlobalNetworkPacketStatModifierReward reward = (GlobalNetworkPacketStatModifierReward)modifierBase;

                if (!networkLine.IsExpanded())
                {
                    networkLine.Expand();
                }

                foreach (UIPanelLine line in networkLine.GetLines())
                {

                    if (line.GetId() == reward.NetworkPacketType.ToString())
                    {
                        (line as UIStatCollectionPanelLine).Preview(modifierBase);
                    }
                }

            }
            else if (modifierBase is WorldObjectTypeNetworkPacketStatModifierReward)
            {
                WorldObjectTypeNetworkPacketStatModifierReward reward =
                    (WorldObjectTypeNetworkPacketStatModifierReward)modifierBase;

                if (!worldObjectTypesLine.IsExpanded())
                {
                    worldObjectTypesLine.Expand();
                }

                UIPanelLine worldObjectTypeLine = worldObjectTypesLine.GetLineById(reward.WorldObjectType.ToString());
                if (worldObjectTypeLine == null)
                {
                    Debug.LogWarning(
                        $"Could not find {reward.WorldObjectType.ToString()} in {worldObjectTypesLine.GetLines().Count}");
                    return;
                }

                if (!worldObjectTypeLine.IsExpanded())
                {
                    worldObjectTypeLine.Expand();
                }

                UIPanelLine worldObjectTypeNetworkPacketDataLine =
                    worldObjectTypeLine.GetLineById(LineType.NetworkPacketData.ToString());
                if (worldObjectTypeNetworkPacketDataLine == null)
                {
                    throw new SystemException(
                        $"Could not find {LineType.NetworkPacketData.ToString()} in {worldObjectTypeLine.GetLines().Count}");
                }

                if (!worldObjectTypeNetworkPacketDataLine.IsExpanded())
                {
                    worldObjectTypeNetworkPacketDataLine.Expand();
                }

                UIPanelLine worldObjectTypeNetworkPacketDataStatsLine =
                    worldObjectTypeNetworkPacketDataLine.GetLineById(reward.NetworkPacketType.ToString());
                if (worldObjectTypeNetworkPacketDataStatsLine == null)
                {
                    throw new SystemException(
                        $"Could not find {reward.NetworkPacketType.ToString()} in {worldObjectTypeNetworkPacketDataLine.GetLines().Count}");
                }

                if (!worldObjectTypeNetworkPacketDataStatsLine.IsExpanded())
                {
                    worldObjectTypeNetworkPacketDataStatsLine.Expand();
                }

                (worldObjectTypeNetworkPacketDataStatsLine as UIStatCollectionPanelLine).Preview(modifierBase);

            }else {
                throw new NotImplementedException();
            }
            
        }
    }
}