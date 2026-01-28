using UnityEngine;

public interface iAttackable
{
    public void ReceiveAttack(NPCBase npcBase);
    Transform transform { get; }
    string name { get; }
    public bool IsDead();
}
