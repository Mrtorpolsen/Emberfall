using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Game/Ability Definition")]
public class AbilityDefinition : LoadoutDefinition
{
    [Header("UI")]
    [SerializeField] private int cost;

    public float Cost => cost;

    public AbilityAction[] actions;

    public float cooldown; // In seconds, 0 for no cooldown
}
