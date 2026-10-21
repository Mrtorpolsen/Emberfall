using UnityEngine;

//When should the unit use the ability.
public class UnitAbilityComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BaseUnitStats unitStats;


    [Header("Unit Abilities")]
    [SerializeField] private UnitAbility minorAbility;
    [SerializeField] private UnitAbility majorAbility;

    private void Awake()
    {
        if (minorAbility.ability != null)
        {
            SetupUnitAbility(minorAbility);
        }

        if (majorAbility.ability != null)
        {
            SetupUnitAbility(majorAbility);
        }
    }

    private void SetupUnitAbility(UnitAbility unitAbility)
    {
        unitAbility.trigger = AbilityTriggerFactory.Create(
            unitAbility.triggerType,
            unitStats,
            unitAbility.ability
        );

        unitAbility.triggeredHandler = () => TriggerAbility(unitAbility.ability);

        unitAbility.trigger.Triggered += unitAbility.triggeredHandler;
        unitAbility.trigger.Initialize();
    }

    private void TriggerAbility(AbilityDefinition ability)
    {
        AbilityContext context = new AbilityContext(
            TargetRegistry.Instance,
            unitStats
        );

        AbilitySystem.Instance.TryExecute(ability, context);
    }

    private void CleanupUnitAbility(UnitAbility unitAbility)
    {
        if (unitAbility?.trigger == null)
            return;

        unitAbility.trigger.Triggered -= unitAbility.triggeredHandler;
        unitAbility.trigger.Dispose();
    }

    private void OnDestroy()
    {
        CleanupUnitAbility(minorAbility);
        CleanupUnitAbility(majorAbility);
    }
}