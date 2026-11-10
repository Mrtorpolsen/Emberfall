using System;

public class CurrencyManager : GlobalSystem<CurrencyManager>
{
    public event Action<int> OnCindersChanged;
    public event Action<CurrencyTypes> OnPlayerCurrencyChanged;

    protected override void Awake()
    {
        base.Awake();
    }

    public int Get(CurrencyTypes type)
    {
        EnsureCurrencyAvailable();
        return SaveService.Instance.Current.Progress.Currency.Get(type);
    }

    public void Add(CurrencyTypes type, int amount)
    {
        EnsureCurrencyAvailable();
        SaveService.Instance.Current.Progress.Currency.Add(type, amount);
        SaveService.Instance.Save();
        OnPlayerCurrencyChanged?.Invoke(type);
        RaiseIfCinders(type);
    }

    public bool Spend(CurrencyTypes type, int amount)
    {
        EnsureCurrencyAvailable();
        bool success = SaveService.Instance.Current.Progress.Currency.Spend(type, amount);

        if (success)
        {
            SaveService.Instance.Save();
            RaiseIfCinders(type);
            OnPlayerCurrencyChanged?.Invoke(type);
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
        if (SaveService.Instance.Current.Progress.Currency == null)
        {
            throw new InvalidOperationException("CurrencyData is null.");
        }
    }
}
