using UnityEngine;

namespace Items
{
    public class ItemBase : MonoBehaviour
    {
        public virtual void Use()
        {
            
        }

        public virtual string UseVerb()
        {
            return "Use";
        }
    }
}