using System;
using System.Collections.Generic;

[Serializable]
public class PlayerTalentState
{
    public Dictionary<string, UnitSaveData> Purchases = new();
    public Dictionary<CurrencyTypes, int> CurrencySpent = new();
}
public class UnitSaveData
{
    public Dictionary<string, int> PurchasedTalents;
}
