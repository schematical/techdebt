using Infrastructure;
using System.Collections.Generic;
using UnityEngine;

public class InternetPipeWOType : WorldObjectType
{
    public InternetPipeWOType()
    {
        type = WorldObjectType.Type.InternetPipe;
        DisplayName = "Internet Pipe";
        PrefabId = "InternetPipe";
        NetworkConnections = new List<NetworkConnection>()
        {
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.ApplicationServer,
                networkPacketType = NetworkPacketData.PType.Text
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.ApplicationServer,
                networkPacketType = NetworkPacketData.PType.Image
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.ApplicationServer,
                networkPacketType = NetworkPacketData.PType.PII
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.ApplicationServer,
                networkPacketType = NetworkPacketData.PType.MaliciousText
            },
            
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.ApplicationServer,
                networkPacketType = NetworkPacketData.PType.Purchase
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.ApplicationServer,
                networkPacketType = NetworkPacketData.PType.SQLInjection
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.ALB,
                networkPacketType = NetworkPacketData.PType.Text,
                priority = 6
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.CDN,
                networkPacketType = NetworkPacketData.PType.Image,
                priority = 6
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.ALB,
                networkPacketType = NetworkPacketData.PType.PII,
                priority = 6
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.ALB,
                networkPacketType = NetworkPacketData.PType.MaliciousText,
                priority = 6
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.ALB,
                networkPacketType = NetworkPacketData.PType.Purchase,
                priority = 6
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.ALB,
                networkPacketType = NetworkPacketData.PType.SQLInjection,
                priority = 6
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.CDN,
                networkPacketType = NetworkPacketData.PType.Image,
                priority = 7
            }
            
        };
    }
}
