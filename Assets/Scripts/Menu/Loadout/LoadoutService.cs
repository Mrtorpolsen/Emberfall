using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEngine;

public class LoadoutService : MonoBehaviour
{
    public static LoadoutService Instance { get; private set; }

    public class ActiveLoadout
    {
        public SpawnDefinition[] UnitLoadout = new SpawnDefinition[4];
        public SpawnDefinition[] TowerLoadout = new SpawnDefinition[3];
        public AbilityDefinition[] AbilityLoadout = new AbilityDefinition[2];
    }

    public int CurrentLoadoutId => SaveService.Instance.Current.Loadouts.ActiveLoadoutId;
    public ActiveLoadout CurrentLoadout { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (SaveService.Instance != null)
        {
            SaveService.Instance.OnSaveLoaded += HandleSaveLoadedAsync;
        }
    }

    private void OnDisable()
    {
        if (SaveService.Instance != null)
        {
            SaveService.Instance.OnSaveLoaded -= HandleSaveLoadedAsync;
        }
    }

    private Task HandleSaveLoadedAsync()
    {
        return LoadPlayerLoadoutAsync();
    }

    private async Task LoadPlayerLoadoutAsync()
    {
        if(LoadoutDatabase.Instance == null)
        {
            Debug.LogError("LoadoutDatabase not found");
            return;
        }

        if (SaveService.Instance.Current.Loadouts.Presets.Count == 0)
        {
            await SetDefaultLoadout();
            return;
        }
        else
        {
            var activePreset = SaveService.Instance.Current.Loadouts.Presets.Find(p => p.Id == CurrentLoadoutId);

            if (activePreset != null)
            {
                CurrentLoadout = new ActiveLoadout
                {
                    UnitLoadout = Array.ConvertAll(activePreset.State.UnitLoadout, id => LoadoutDatabase.Instance.GetSpawn(id)),
                    TowerLoadout = Array.ConvertAll(activePreset.State.TowerLoadout, id => LoadoutDatabase.Instance.GetSpawn(id)),
                    AbilityLoadout = Array.ConvertAll(activePreset.State.AbilityLoadout, id => LoadoutDatabase.Instance.GetAbility(id))
                };
            } 
            else
            {
                Debug.LogWarning($"Active loadout preset with ID {CurrentLoadoutId} not found. Setting default loadout.");
                await SetDefaultLoadout();
            }
        }

    }

    private async Task SetDefaultLoadout()
    {
        SaveService.Instance.Current.Loadouts.ActiveLoadoutId = GenerateLoadoutId();

        CurrentLoadout = new ActiveLoadout
        {
            UnitLoadout = new SpawnDefinition[]
            {
                LoadoutDatabase.Instance.GetSpawn("spawn_fighter"),
                LoadoutDatabase.Instance.GetSpawn("spawn_ranger"),
                LoadoutDatabase.Instance.GetSpawn("spawn_cavalier"),
                null
            },
            TowerLoadout = new SpawnDefinition[]
            {
                LoadoutDatabase.Instance.GetSpawn("spawn_tower"),
                LoadoutDatabase.Instance.GetSpawn("spawn_ballista"),
                LoadoutDatabase.Instance.GetSpawn("spawn_bomb")
            },
            AbilityLoadout = new AbilityDefinition[]
            {
                LoadoutDatabase.Instance.GetAbility("ability_fortify"),
                LoadoutDatabase.Instance.GetAbility("ability_restoration")
            }
        };

        var state = new PlayerLoadoutState
        {
            UnitLoadout = new[] { "spawn_fighter", "spawn_ranger", "spawn_cavalier", null },
            TowerLoadout = new[] { "spawn_tower", "spawn_ballista", "spawn_bomb" },
            AbilityLoadout = new[] { "ability_fortify", "ability_restoration" }
        };

        SaveService.Instance.Current.Loadouts.Presets.Add(
            new SavedLoadout { 
                Id = CurrentLoadoutId,
                DisplayName = "Default Loadout",
                State = state
            }
        );

        await SaveService.Instance.SaveAsync();
    }

    private int GenerateLoadoutId()
    {
        return SaveService.Instance.Current.Loadouts.NextLoadoutId++;
    }
}
