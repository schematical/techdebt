namespace DefaultNamespace
{
    public class LocalizationData
    {
        public string Name;
        public string ToolTip;
    }
    public class LocalizationManager
    {
        public LocalizationData GetStatTypeData(StatType statType)
        {
            switch (statType)
            {
                case(StatType.Difficulty):
                    return new LocalizationData() {
                        Name = "Difficulty", 
                        ToolTip = "`Difficulty` affects things like how quickly traffic or tech debt increases."
                    };
                case(StatType.TechDebt):
                    return new LocalizationData() {
                        Name = "TechDebt", 
                        ToolTip = "`TechDebt` determines likely hood of things bad things happening like cyber attacks."
                    };
                case(StatType.Global_LevelUpRarityModifier):
                    return new LocalizationData() {
                        Name = "Rarity", 
                        ToolTip = "`Rarity` determines the rarity of level ups for your team and deployment rewards."
                    };
                case(StatType.Traffic):
                    return new LocalizationData() {
                        Name = "Traffic", 
                        ToolTip = "`Traffic` determines the rate at which Network Packets flow in from the internet."
                    };
                case(StatType.PacketsSent):
                    return new LocalizationData() {
                        Name = "PacketsSent", 
                        ToolTip = "`PacketsSent` counts the amount of packets sent today."
                    };
                case(StatType.PacketsSucceeded):
                    return new LocalizationData() {
                        Name = "PacketsSucceeded", 
                        ToolTip = "`PacketsSucceeded` counts the amount of packets that have successfully made it through your server infrastructure today."
                    };
                case(StatType.PacketsFailed):
                    return new LocalizationData() {
                        Name = "PacketsFailed", 
                        ToolTip = "`PacketsFailed` counts the amount of packets that have failed to make it through your server infrastructure today."
                    };
                case(StatType.Infra_MaxLoad):
                    return new LocalizationData() {
                        Name = "MaxLoad", 
                        ToolTip = "`MaxLoad` tells you the maximum amount of load from Network Packets this piece of infrastructure can take before it freezes."
                    };
                case(StatType.Infra_LoadRecoveryRate):
                    return new LocalizationData() {
                        Name = "LoadRecoveryRate", 
                        ToolTip = "`LoadRecoveryRate` tells you the rate at which this piece of infrastructure recovers from Network Packets."
                    };
                case(StatType.Infra_BuildTime):
                    return new LocalizationData() {
                        Name = "BuildTime", 
                        ToolTip = "`BuildTime` is the duration of time it takes to spin up a piece of infrastructure."
                    };
                case(StatType.Infra_DailyCost):
                    return new LocalizationData() {
                        Name = "DailyCost", 
                        ToolTip = "`DailyCost` is what a piece of infrastructure costs each day. NOTE: You get billed as soon as you reach the minimum cost rounded to the nearest dollar."
                    };
                case(StatType.Infra_LoadPerPacket):
                    return new LocalizationData() {
                        Name = "Load Per Packet", 
                        ToolTip = "`Load Per Packet` is the amount of load a specific type of Network Packet will add to the infrastructure that services it."
                    };
                case(StatType.Infra_PacketCost):
                    return new LocalizationData() {
                        Name = "PacketCost", 
                        ToolTip = "`PacketCost` is what a network packet costs when it is serviced by this infrastructure."
                    };
                case(StatType.NPC_MovementSpeed):
                    return new LocalizationData() {
                        Name = "MovementSpeed", 
                        ToolTip = "`MovementSpeed` determines how fast your team member can move."
                    };
                case(StatType.NPC_XPSpeed):
                    return new LocalizationData() {
                        Name = "XPSpeed", 
                        ToolTip = "`XPSpeed` determines how fast your team member gains XP and levels up."
                    };
                case(StatType.NPC_DevOpsSpeed):
                    return new LocalizationData() {
                        Name = "DevOps Speed", 
                        ToolTip = "`DevOps Speed` determines how fast your team member can spin up or resize new infrastructure."
                    };
                case(StatType.NPC_ResearchSpeed):
                    return new LocalizationData() {
                        Name = "Research Speed", 
                        ToolTip = "`Research Speed` determines how fast your team member can research new Technology."
                    };
                case(StatType.NPC_ModifierSlots):
                    return new LocalizationData() {
                        Name = "Modifier Slots", 
                        ToolTip = "`Modifier Slots` determines how many traits you can have when you level up."
                    };
                case(StatType.Infra_MaxSize):
                    return new LocalizationData() {
                        Name = "Max Size", 
                        ToolTip = "`Max Size` determines how big you can resize your infrastructure."
                    };
                case(StatType.NPC_CodeQuality):
                    return new LocalizationData() {
                        Name = "Code Quality", 
                        ToolTip = "`Code Quality` determines the likely hood that your deployments will have higher rarity rewards."
                    };
                case(StatType.NPC_HP):
                    return new LocalizationData() {
                        Name = "HP", 
                        ToolTip = "`HP` how much stress your team member can take until they have a bread down and leave."
                    };
                case(StatType.NPC_CodeSpeed):
                    return new LocalizationData() {
                        Name = "Code Speed", 
                        ToolTip = "`Code Speed` determines spead at which your team member can code new releases for deployment."
                    };
                case(StatType.NPC_CoolDown):
                    return new LocalizationData() {
                        Name = "Cool Down", 
                        ToolTip = "`Cool Down` determines your team member's cool down on various tasks."
                    };
                case(StatType.AttackPossibility):
                    return new LocalizationData() {
                            Name = "Attack Possibility", 
                            ToolTip = "`Attack Possibility` ticks up over time and determines the chances of something bad happening."
                        };  
                case(StatType.Global_AttackPossibilityAccumulationRate):
                    return new LocalizationData() {
                            Name = "Attack Possibility Accumulation Rate", 
                            ToolTip = "`Attack Possibility Accumulation Rate` determines rate at which the `Attack Possibility` stat picks up."
                        };     
                case(StatType.NPC_AttackDamage):
                    return new LocalizationData() {
                        Name = "Debug Speed", 
                        ToolTip = "`Debug Speed` determines how quickly your team member can debug an issue."
                    };   
                case(StatType.TechDebt_AccumulationRate):
                    return new LocalizationData() {
                        Name = "Tech Debt Accumulation Rate", 
                        ToolTip = "`Tech Debt Accumulation Rate` determines how fast `Tech Debt` accumulates."
                    };   
                case(StatType.NetworkPacket_Probability):
                    return new LocalizationData() {
                        Name = "Network Packet Probability", 
                        ToolTip = "`Network Packet Probability` determines the probability that a network packet will come in via the internet."
                    };
                case(StatType.NPC_FixSpeed):
                    return new LocalizationData() {
                        Name = "Fix Speed", 
                        ToolTip = "`Fix Speed` determines the speed at which your team member will fix frozen infrastructure."
                    };  
                case(StatType.Infra_LatencyStartsAtLoad):
                    return new LocalizationData() {
                        Name = "Latency Starts At Load", 
                        ToolTip = "`Latency Starts At Load` determines when infrastructure will start slowing down the rate at which Network Packets travel increasing system latency."
                    }; 
                case(StatType.NetworkPacket_LoadLatencyMultiplier):
                    return new LocalizationData() {
                        Name = "Load Latency Multiplier", 
                        ToolTip = "`Load Latency Multiplier` increases the effects of `Latency` on this type of Network Packet."
                    };
                case(StatType.Global_ReleaseQualityMultiplier):
                    return new LocalizationData() {
                        Name = "Release Quality Multiplier", 
                        ToolTip = "`Release Quality Multiplier` determines the quality of future code releases which affects things like reward rarity an the chances of bad things happening from Tech Debt."
                    }; 
                case(StatType.NPC_Release_TechDebt):
                    return new LocalizationData() {
                        Name = "Release Tech Debt", 
                        ToolTip = "`Release Tech Debt` determines the affect your team member will have on Tech Debt when contributing to a code release. This can be good or bad."
                    }; 
                case(StatType.Global_CodeSpeed):
                    return new LocalizationData() {
                        Name = "Global Code Speed", 
                        ToolTip = "`Global Code Speed` determines the speed at which your team members contribute to a code release."
                    }; 
                case(StatType.Global_DeploymentSpeed):
                    return new LocalizationData() {
                        Name = "Global Deployment Speed", 
                        ToolTip = "`Global Deployment Speed` determines the speed at which your team members can deploy a code release."
                    }; 
                case(StatType.Global_PIILossCost):
                    return new LocalizationData() {
                        Name = "PII Loss Cost", 
                        ToolTip = "`PII Loss Cost` determines approximate cost of having PPI packets or purchases compromised by malicious parties."
                    }; 
                case(StatType.Global_ReRolls):
                    return new LocalizationData() {
                        Name = "ReRolls", 
                        ToolTip = "`ReRolls` determines how many re-rolls you have to re-roll deployment rewards and team member level ups."
                    }; 
                case(StatType.Global_Banish):
                    return new LocalizationData() {
                        Name = "Banish", 
                        ToolTip = "`Banish` determines how many banishes you have to remove deployment rewards and team member level ups from this run."
                    }; 
                case(StatType.NPC_StartLevel):
                    return new LocalizationData() {
                        Name = "Start Level", 
                        ToolTip = "`Start Level` determines the level team members start at during each run."
                    }; 
                case(StatType.NPC_LevelUpRarity):
                    return new LocalizationData() {
                        Name = "Level Up Rarity", 
                        ToolTip = "`Level Up Rarity` determines the rarity of team members level ups."
                    }; 
                case(StatType.NPC_ContractWorkMoneyMultiplier):
                    return new LocalizationData() {
                        Name = "Contract Work Money Multiplier", 
                        ToolTip = "`Contract Work Money Multiplier` affects how much money you make when doing `Contract Work`."
                    }; 
                case(StatType.NPC_BugAttackTechDebtMultiplier):
                    return new LocalizationData() {
                        Name = "Bug Attack Tech Debt Multiplier", 
                        ToolTip = "`Bug Attack Tech Debt Multiplier` affects how much, if any, Tech Debt is paid down when your team member is debugging."
                    }; 
                case(StatType.Global_DailyBudget):
                    return new LocalizationData() {
                        Name = "Daily Budget", 
                        ToolTip = "`Daily Budget` affects how much money is added to your budget each day."
                    }; 
             case(StatType.Infra_InputValidation):
                    return new LocalizationData() {
                        Name = "Input Validation", 
                        ToolTip = "`Input Validation` decreases the damage done by XSS attacks."
                    }; 
                /*


        NetworkPacket_ValueMin,
        NetworkPacket_ValueMax,
        TotalNetworkPacketLatency,
        VictoryCondition_NetworkPacketLatency,
                     */
                default:
                    throw new System.NotImplementedException($"Missing localization for statType `{statType}`.");
                
            }
        }
    }
}