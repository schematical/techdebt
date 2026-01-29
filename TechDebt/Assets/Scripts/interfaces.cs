using UnityEngine;

public interface iTargetable
{

    Transform transform { get; }
    string name { get; }
    GameObject gameObject { get;  }
}
public interface iAttackable: iTargetable
{
    public void ReceiveAttack(NPCBase npcBase);
    Transform transform { get; }
    string name { get; }
    public bool IsDead();
}