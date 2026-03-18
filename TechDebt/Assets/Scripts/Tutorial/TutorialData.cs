using System.Collections.Generic;

namespace Tutorial
{
    [System.Serializable]
    public class TutorialData
    {

        public List<TutorialStepData> steps = new();
    }
    [System.Serializable]
    public class TutorialStepData
    {
        public string Id;
        public bool completed;
    }
}