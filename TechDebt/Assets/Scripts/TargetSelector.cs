using System;
using Infrastructure;
using UnityEngine;

namespace DefaultNamespace
{
    public class TargetSelector
    {
        public TargetType Type;
        public string Id;

        public Transform GetTransform()
        {
            switch (Type)
            {
                case(TargetType.WorldObject):
                    WorldObjectBase worldObjectBase = GameManager.Instance.GetInfrastructureInstanceByID(Id);
                    if (worldObjectBase == null)
                    {
                        Debug.LogError($"Could not find `{Type}` `{Id}`");
                    }
                    return worldObjectBase.transform;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}