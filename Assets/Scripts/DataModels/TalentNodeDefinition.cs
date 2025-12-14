using System;
using UnityEditor;

public class TalentNodeDefinition
{
    public string img;
    public string heading;
    public string description;
    public string unlocked;
    public int tier;
    public float cost;
    public PopupButtonDefinition buttonDefinition;
    public Action onClick;
}
