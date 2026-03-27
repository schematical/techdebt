using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Tutorial
{
    [System.Serializable]
    public class TutorialData
    {

        public List<TutorialStepData> steps = new();
        [FormerlySerializedAs("State")] public TutorialManager.TutorialManagerState state;
    }
    [System.Serializable]
    public class TutorialStepData
    {
        public TutorialStepId Id;
        public TutorialStep.TutorialStepState State;
    }
}