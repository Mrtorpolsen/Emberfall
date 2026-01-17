using System;
using System.Collections.Generic;

[Serializable]
public class PlayerTalentState
{
    public Dictionary<string, int> Purchases = new();
    public Dictionary<CurrencyTypes, int> CurrencySpent = new();
}
