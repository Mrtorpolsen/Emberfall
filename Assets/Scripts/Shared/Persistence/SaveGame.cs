using System;

[Serializable]
public class SaveGame
{
    public int Version = 1;

    public PlayerTalentState Talents = new();
    public PlayerResearchState Research = new();
    public CurrencyData Currency = new();

    public bool HasReceivedLoginGift = false;
}
