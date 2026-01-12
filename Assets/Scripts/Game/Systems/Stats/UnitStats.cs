using UnityEngine;

public abstract class UnitStats : MonoBehaviour
{
    public abstract Team Team { get; set; }
    public abstract float Cost { get; }
}