// UnlockCondition.cs
using System;
using DefaultNamespace;
using Tutorial;
using UnityEngine.Serialization;

[Serializable]
public class UnlockCondition: iUnlockable
{

    public enum ConditionType { Technology, SprintGreaterOrEqual, TutorialStepState, PrestigePointAllocation, GlobalNetworkPacket, GameStage }

    public ConditionType Type;
    public int SprintNumber;
    [FormerlySerializedAs("TechnologyID")] public string TargetId;
    public TutorialStepId TutorialStepId = TutorialStepId.None;
    public TutorialStep.TutorialStepState TutorialStepState = TutorialStep.TutorialStepState.Completed;
    public GameStage? gameStage = null;

    public bool IsUnlocked()
    {
        switch (Type)
        {
            case(ConditionType.Technology):
                Technology technology = GameManager.Instance.GetTechnologyByID(TargetId);
                if (technology == null)
                {
                    return false;
                }

                if (technology.CurrentState != Technology.State.Unlocked)
                {
                    return false;
                }
                return true;
            case(ConditionType.TutorialStepState):
                if (TutorialStepId == TutorialStepId.None)
                {
                    throw new SystemException("Invalid tutorial step state");
                }

                if (
                    GameManager.Instance.TutorialManager == null
                )
                {
                    return true;
                }
                return GameManager.Instance.TutorialManager.GetStep(TutorialStepId).State == TutorialStepState;
            case(ConditionType.SprintGreaterOrEqual):
                return GameManager.Instance.Map.CurrentSprintNumber >= SprintNumber; 
            case(ConditionType.PrestigePointAllocation):
                MetaProgressData progress = MetaGameManager.GetProgress();
                return progress.prestigePointAllocations.Find((allocation) => allocation.Id == TargetId) != null;
            case(ConditionType.GlobalNetworkPacket):
                NetworkPacketData networkPacketData = GameManager.Instance.GetNetworkPacketDatas()
                    .Find((networkPacketData => networkPacketData.Type.ToString() == TargetId));
                if (networkPacketData == null)
                {
                    throw new SystemException($"Invalid network packet data: ${TargetId}");
                }
                return networkPacketData.GetProbability() > 0;
            case(ConditionType.GameStage):
                MetaProgressData metaProgressData = MetaGameManager.GetProgress();
                if (gameStage == null)
                {
                    throw new SystemException("Invalid game stage");
                }

                return (metaProgressData.gameStage >= gameStage);
            default:
                throw new NotImplementedException();
        }
    }

    public override string ToString()
    {
        switch (Type)
        {
            case(ConditionType.Technology):
                return GameManager.Instance.GetTechnologyByID(TargetId).DisplayName;
            case(ConditionType.SprintGreaterOrEqual):
                return$"Sprint {SprintNumber} Or Greater";   
            case(ConditionType.TutorialStepState):
                return$"Tutorial Step {TutorialStepId} is {TutorialStepState}";
            default:
                throw new NotImplementedException();
        }
    }
    
    public string GetDescription()
    {
        switch (Type)
        {
            case ConditionType.Technology:
                return $"Requires Technology_Locked {TargetId}";
            default:
                return "Unknown requirement";
        }
    }
}

