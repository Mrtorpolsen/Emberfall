using System;
using UnityEngine.UIElements;

public class TalentNodeDefinition
{
    public string img;
    public string heading;
    public string description;
    public string purchased;
    public int tier;
    public float cost;
    public PopupButtonDefinition buttonDefinition;
    public Action onClick;

    public Label purchasedLabel;
    public Action<int, int> UpdatePurchasedText;
}
