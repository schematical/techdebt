using Infrastructure;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public class CodePipelineWOType : WorldObjectType
{
    public CodePipelineWOType()
    {
        type = WorldObjectType.Type.CodePipeline;
        DisplayName = "Code Pipeline";
        PrefabId = "CodePipeline";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = false;
        ShowInGlobalDisplay = true;
        TutorialStepId = TutorialStepId.Infra_CodePipeline_Tip;
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TechnologyID = "codepipeline"
            }
        };
    }
}
