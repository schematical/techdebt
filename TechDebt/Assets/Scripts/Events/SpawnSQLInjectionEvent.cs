using System;
using NPCs;
using UnityEngine;

namespace Tutorial
{
    public class SpawnSQLInjectionEvent: EventBase
    {
        public override void Apply()
        {
            Debug.Log("SpawnSQLInjectionEvent.Apply()");
            InternetPipe internetPipe = GameManager.Instance.GetRandomInfrastructureInstanceByClass<InternetPipe>();
            NetworkPacketData data =
                GameManager.Instance.GetNetworkPacketDataByType(NetworkPacketData.PType.SQLInjection);
            NetworkPacket networkPacket = internetPipe.SendPacket(data);
            GameManager.Instance.SetStat(StatType.AttackPossibility, 0);
        }
        public override float GetProbability()
        {
            GameManager gameManager = GameManager.Instance;
            ReleaseBase currentRelease = gameManager.GetCurrentRelease();
            if (currentRelease == null)
            {
                return 0;
            }

            float techDebt = gameManager.GetStatValue(StatType.TechDebt);
            
            float releaseQuality = 1 - currentRelease.GetQuality();
            
            float attackPossibility = gameManager.GetStatValue(StatType.AttackPossibility);
            return (techDebt * releaseQuality * attackPossibility);
        }
        

    }
}