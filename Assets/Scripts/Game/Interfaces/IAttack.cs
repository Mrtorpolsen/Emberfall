// How a unit attacks once its target is in range and the cooldown is ready.
// Units without one attack in melee (CombatComponent damages the target directly).
public interface IAttack
{
    void Attack(ITargetable target);
}
