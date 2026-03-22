// UnlockCondition.cs
using System;
using Tutorial;

[Serializable]
public class UnlockCondition
{

    public enum ConditionType { Technology, SprintGreaterOrEqual, TutorialStepState }

    public ConditionType Type;
    public int SprintNumber;
    public string TechnologyID;
    public TutorialStepId TutorialStepID = TutorialStepId.None;
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
                if (TutorialStepID == TutorialStepId.None)
                {
                    throw new SystemException("Invalid tutorial step state");
                }

                return GameManager.Instance.TutorialManager.GetStep(TutorialStepID).State == TutorialStepState;
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
                return$"Tutorial Step {TutorialStepID} is {TutorialStepState}";
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

