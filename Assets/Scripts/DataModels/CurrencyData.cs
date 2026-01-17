using System;

[Serializable]
public class CurrencyData
{
    public int Cinders;
    public int Embers;

    public int Get(CurrencyTypes type)
    {
        return type switch
        {
            CurrencyTypes.Cinders => Cinders,
            CurrencyTypes.Embers => Embers,
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown currency type: {type}")
        };
    }

    public void Add(CurrencyTypes type, int amount)
    {
        switch (type)
        {
            case CurrencyTypes.Cinders: Cinders += amount; break;
            case CurrencyTypes.Embers: Embers += amount; break;
            default: throw new ArgumentOutOfRangeException(nameof(type), $"Unknown currency type: {type}");
        }
    }

    public bool Spend(CurrencyTypes type, int amount)
    {
        switch (type)
        {
            case CurrencyTypes.Cinders:
                if (Cinders < amount) return false;
                Cinders -= amount;
                return true;
            case CurrencyTypes.Embers:
                if (Embers < amount) return false;
                Embers -= amount;
                return true;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), $"Unknown currency type: {type}");
        }
    }
}