using System;

public class CurrencyManager : GlobalSystem<CurrencyManager>
{
    public event Action<int> OnCindersChanged;

    protected override void Awake()
    {
        base.Awake();
    }

    public int Get(CurrencyTypes type)
    {
        EnsureCurrencyAvailable();
        return SaveService.Instance.Current.Currency.Get(type);
    }

    public void Add(CurrencyTypes type, int amount)
    {
        EnsureCurrencyAvailable();
        SaveService.Instance.Current.Currency.Add(type, amount);
        SaveService.Instance.Save();
        RaiseIfCinders(type);
    }

    public bool Spend(CurrencyTypes type, int amount)
    {
        EnsureCurrencyAvailable();
        bool success = SaveService.Instance.Current.Currency.Spend(type, amount);

        if (success)
        {
            SaveService.Instance.Save();
            RaiseIfCinders(type);
        }

        return success;
    }

    private void RaiseIfCinders(CurrencyTypes type)
    {
        if (type != CurrencyTypes.Cinders)
            return;

        OnCindersChanged?.Invoke(Get(CurrencyTypes.Cinders));
    }

    private void EnsureCurrencyAvailable()
    {
        if (SaveService.Instance == null)
        {
            throw new InvalidOperationException("SaveService.Instance is null.");
        }
        if (SaveService.Instance.Current == null)
        {
            throw new InvalidOperationException("SaveService.Current is null.");
        }
        if (SaveService.Instance.Current.Currency == null)
        {
            throw new InvalidOperationException("CurrencyData is null.");
        }
    }
}
