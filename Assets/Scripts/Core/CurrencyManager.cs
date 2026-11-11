using System;

// Read-only view of the player's Cinders and Embers. The server owns them (docs/cloud-save.md): they change
// only through server actions (ServerActions), which call NotifyChanged.
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

    // After the progress was replaced (a server answer): both balances may have changed
    public void NotifyChanged()
    {
        foreach (CurrencyTypes type in Enum.GetValues(typeof(CurrencyTypes)))
        {
            OnPlayerCurrencyChanged?.Invoke(type);
        }

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
