using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ability Actions/Summon")]
public class SummonAction : AbilityAction
{
    public GameObject summonPrefab;

    public override void Execute(AbilityContext context)
    {
        if (AbilityCooldownManager.Instance == null)
        {
            Debug.LogError($"AbilityCooldownManager instance is null. \n Name: {context.Caster.name}  \n {abilityDefinition.name}");
            return;
        }

        if (!AbilityCooldownManager.Instance.CanUse(abilityDefinition, context.Caster))
            return;

        SpawnManager.Instance.SpawnUnit(summonPrefab, context.Caster.Transform, Team.South);
    }
}
