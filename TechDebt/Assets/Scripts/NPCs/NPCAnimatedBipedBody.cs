using UnityEngine;

namespace NPCs
{
    public class NPCAnimatedBipedBody: MonoBehaviour
    {
        public NPCAnimatedBiped biped;

        public void EndAttackAnimation()
        {
            biped.EndAttackAnimation();
        }

        public void HitAttackAnimation()
        {
            biped.HitAttackAnimation();
        }
    }
}