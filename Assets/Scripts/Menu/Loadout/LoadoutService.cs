using System;
using System.Collections;
using System.Collections.Generic;
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

        public IEnumerable<LoadoutSlot> EnumerateSlots()
        {
            for (int i = 0; i < UnitLoadout.Length; i++)
            {
                yield return new LoadoutSlot
                {
                    SlotType = DefinitionCategory.Unit,
                    Index = i,
                    Definition = UnitLoadout[i]
                };
            }

            for (int i = 0; i < TowerLoadout.Length; i++)
            {
                yield return new LoadoutSlot
                {
                    SlotType = DefinitionCategory.Tower,
                    Index = i,
                    Definition = TowerLoadout[i]
                };
            }

            for (int i = 0; i < AbilityLoadout.Length; i++)
            {
                yield return new LoadoutSlot
                {
                    SlotType = DefinitionCategory.Utility,
                    Index = i,
                    Definition = AbilityLoadout[i]
                };
            }
        }
        public ActiveLoadout Clone()
        {
            return new ActiveLoadout
            {
                UnitLoadout = (SpawnDefinition[])UnitLoadout.Clone(),
                TowerLoadout = (SpawnDefinition[])TowerLoadout.Clone(),
                AbilityLoadout = (AbilityDefinition[])AbilityLoadout.Clone()
            };
        }
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

        await SaveLoadout(CurrentLoadout);
    }

    public async Task SaveLoadout(ActiveLoadout activeLoadout)
    {
        var state = new PlayerLoadoutState
        {
            UnitLoadout = new string[4],
            TowerLoadout = new string[3],
            AbilityLoadout = new string[2]
        };

        foreach (var slot in activeLoadout.EnumerateSlots())
        {
            var id = slot.Definition?.Id;

            switch (slot.SlotType)
            {
                case DefinitionCategory.Unit:
                    state.UnitLoadout[slot.Index] = id;
                    break;

                case DefinitionCategory.Tower:
                    state.TowerLoadout[slot.Index] = id;
                    break;

                case DefinitionCategory.Utility:
                    state.AbilityLoadout[slot.Index] = id;
                    break;
            }
        }

        var existingPreset =
            SaveService.Instance.Current.Loadouts.Presets
                .Find(p => p.Id == CurrentLoadoutId);

        if (existingPreset != null)
        {
            existingPreset.State = state;
        }
        else
        {
            SaveService.Instance.Current.Loadouts.Presets.Add(
                new SavedLoadout
                {
                    Id = CurrentLoadoutId,
                    DisplayName = $"Loadout {CurrentLoadoutId}",
                    State = state
                }
            );
        }

        CurrentLoadout = activeLoadout.Clone();

        await SaveService.Instance.SaveAsync();
    }

    private int GenerateLoadoutId()
    {
        return SaveService.Instance.Current.Loadouts.NextLoadoutId++;
    }

    public List<LoadoutDefinition> GetAllAvailableLoadoutDefinitions()
    {
        //Todo determine which loadouts are available based on player progress, for now just return all
        return LoadoutDatabase.Instance.GetAllDefinitions();
    }

    public string GetCurrentLoadoutDisplayName()
    {
        var activePreset = SaveService.Instance.Current.Loadouts.Presets.Find(p => p.Id == CurrentLoadoutId);
        return activePreset != null ? activePreset.DisplayName : "Unknown Loadout";
    }
}
