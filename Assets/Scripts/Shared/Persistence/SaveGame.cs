using System;

[Serializable]
public class SaveGame
{
    public const int CURRENT_SAVE_VERSION = 2;
    public int Version;

    public PlayerTalentState Talents = new();
    public PlayerResearchState Research = new();
    public CurrencyData Currency = new();

    public bool HasReceivedLoginGift = false;
}
