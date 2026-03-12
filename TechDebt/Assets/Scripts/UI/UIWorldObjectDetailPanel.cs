using System.Collections.Generic;
using System.Linq;
using Infrastructure;
using Stats;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIWorldObjectDetailPanel: UIPanel
    {
        private WorldObjectBase _selectedWorldObject;
        // public UITextArea textArea; // Removed to use AddLine instead
  

        protected override void Update()
        {
            base.Update();
            // Removed textArea update logic
        }
        public void ShowWorldObjectDetail(WorldObjectBase worldObject)
        {
            _selectedWorldObject = worldObject;
            CleanUp(); // Clear existing lines
            base.Show();
            WorldObjectType type = _selectedWorldObject.GetWorldObjectType();
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().h1($"{_selectedWorldObject.GetDisplayName()}");
            

            List<NPCTask> tasks = _selectedWorldObject.GetAvailableTasks();
            foreach (NPCTask task in tasks)
            {
                NPCTask localTask = task; // Local copy for the closure
                AddButton(task.GetAssignButtonText(), () =>
                {
                    GameManager.Instance.AddTask(localTask);
                    _selectedWorldObject.HideAttentionIcon();
                    Close();
                });
            }

        
            
            InfrastructureInstance infraInstance = _selectedWorldObject as InfrastructureInstance;

            if (infraInstance != null)
            {
                // State
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"State: {infraInstance.data.CurrentState}";
                
                // Release
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Release: {infraInstance.Version}";

                // Load
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Curr Load: {infraInstance.CurrentLoad:F2}/{infraInstance.GetMaxLoad():F2}";

                // Daily Cost
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Daily Cost: ${infraInstance.GetDailyCost():F2}";

                // Size
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Curr Size: {infraInstance.CurrentSizeLevel}";
            }

            // Stats
            UIStatCollectionPanelLine statsLine = AddLine<UIStatCollectionPanelLine>();
            statsLine.SetStatCollection(type.Stats, "Stats");
            statsLine.SetId("Stats");

            // Network Packets
            if (type.networkPackets.Count > 0)
            {
                UIPanelLine networkPacketsHeader = AddLine<UIPanelLine>();
                networkPacketsHeader.Add<UIPanelLineSectionText>().text.text = "Network Packets:";
                networkPacketsHeader.SetExpandable((line) =>
                {
                    foreach (InfrastructureDataNetworkPacket packet in type.networkPackets)
                    {
                        UIStatCollectionPanelLine packetLine = line.AddLine<UIStatCollectionPanelLine>();
                        packetLine.SetStatCollection(packet.Stats, packet.PacketType.ToString());
                        packetLine.SetId(packet.PacketType.ToString());
                    }
                });
            }

            // Connections
            if (infraInstance != null)
            {
                UIPanelLine connectionsHeader = AddLine<UIPanelLine>();
                connectionsHeader.Add<UIPanelLineSectionText>().text.text = "Connections:";
                connectionsHeader.SetExpandable((line) =>
                {
                    if (infraInstance.CurrConnections.Count == 0)
                    {
                        line.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "No active connections.";
                    }
                    else
                    {
                        foreach (var kvp in infraInstance.CurrConnections)
                        {
                            UIPanelLine packetConnLine = line.AddLine<UIPanelLine>();
                            packetConnLine.Add<UIPanelLineSectionText>().text.text = $"<b>{kvp.Key}:</b>";
                            packetConnLine.SetExpandable((pLine) =>
                            {
                                foreach (NetworkConnection conn in kvp.Value)
                                {
                                    UIPanelLine connDetailLine = pLine.AddLine<UIPanelLine>();
                                    connDetailLine.Add<UIPanelLineSectionText>().text.text = $"{conn.networkPacketType} -> {conn.worldObjectType}";
                                    
                                    if (conn.networkConnectionBonus.Count > 0)
                                    {
                                        connDetailLine.SetExpandable((bonusLine) =>
                                        {
                                            foreach (NetworkConnectionBonus bonus in conn.networkConnectionBonus)
                                            {
                                                UIPanelLine bLine = bonusLine.AddLine<UIPanelLine>();
                                                bLine.Add<UIPanelLineSectionText>().text.text = $"{bonus.Stat}: {bonus.value:F2}";
                                            }
                                        });
                                    }
                                }
                            });
                        }
                    }
                });
            }
        }
    }
}
