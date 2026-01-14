using UnityEngine;

public abstract class UnitMetadata : MonoBehaviour
{
    public abstract Team Team { get; set; }
    public abstract float Cost { get; }
}