using UnityEngine;

public abstract class LoadoutDefinition : ScriptableObject
{
    [SerializeField] private string id;
    public string Id => id;
}