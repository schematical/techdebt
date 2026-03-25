// UnlockCondition.cs
using System;
using Tutorial;
using UnityEngine.Serialization;

[Serializable]
public class UnlockCondition: iUnlockable
{

    public enum ConditionType { Technology, SprintGreaterOrEqual, TutorialStepState }

    public ConditionType Type;
    public int SprintNumber;
    public string TechnologyID;
    [FormerlySerializedAs("TutorialStepID")] public TutorialStepId TutorialStepId = TutorialStepId.None;
    public TutorialStep.TutorialStepState TutorialStepState = TutorialStep.TutorialStepState.Completed;

    public bool IsUnlocked()
    {
        switch (Type)
        {
            case(ConditionType.Technology):
                Technology technology = GameManager.Instance.GetTechnologyByID(TechnologyID);
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
                    GameManager.Instance.TutorialManager == null ||
                    !GameManager.Instance.TutorialManager.IsActive()
                )
                {
                    return true;
                }
                return GameManager.Instance.TutorialManager.GetStep(TutorialStepId).State == TutorialStepState;
            case(ConditionType.SprintGreaterOrEqual):
                return GameManager.Instance.Map.GetCurrentStage().StageNumber >= SprintNumber;
            default:
                throw new NotImplementedException();
        }
    }

    public override string ToString()
    {
        switch (Type)
        {
            case(ConditionType.Technology):
                return GameManager.Instance.GetTechnologyByID(TechnologyID).DisplayName;
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
                return $"Requires Technology_Locked {TechnologyID}";
            default:
                return "Unknown requirement";
        }
    }
}

