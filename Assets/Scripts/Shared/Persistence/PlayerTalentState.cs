#nullable disable // Unity code is nullable-oblivious; the Cloud Code module compiles this file with nullable on
using System.Collections.Generic;

public class PlayerTalentState
{
    public Dictionary<string, UnitSaveData> Purchases = new();
    public Dictionary<CurrencyTypes, int> CurrencySpent = new();
}
public class UnitSaveData
{
    public Dictionary<string, int> PurchasedTalents;
}
