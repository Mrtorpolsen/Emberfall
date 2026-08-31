using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

    private Dictionary<string, LoadoutDefinition> definitionMap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        definitionMap = AbilityDatabase.Instance.GetAllAbilities()
            .Cast<LoadoutDefinition>()
            .Concat(SpawnDatabase.Instance.GetAllSpawns())
            .ToDictionary(x => x.Id);
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
        if(SpawnDatabase.Instance == null)
        {
            Debug.LogError("SpawnDatabase not found");
            return;
        }

        if(AbilityDatabase.Instance == null)
        {
            Debug.LogError("AbilityDatabase not found");
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
                    UnitLoadout = Array.ConvertAll(activePreset.State.UnitLoadout, id => SpawnDatabase.Instance.GetSpawn(id)),
                    TowerLoadout = Array.ConvertAll(activePreset.State.TowerLoadout, id => SpawnDatabase.Instance.GetSpawn(id)),
                    AbilityLoadout = Array.ConvertAll(activePreset.State.AbilityLoadout, id => AbilityDatabase.Instance.GetAbility(id))
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
                SpawnDatabase.Instance.GetSpawn("spawn_fighter"),
                SpawnDatabase.Instance.GetSpawn("spawn_ranger"),
                SpawnDatabase.Instance.GetSpawn("spawn_cavalier"),
                null
            },
            TowerLoadout = new SpawnDefinition[]
            {
                SpawnDatabase.Instance.GetSpawn("spawn_tower"),
                SpawnDatabase.Instance.GetSpawn("spawn_ballista"),
                SpawnDatabase.Instance.GetSpawn("spawn_bomb")
            },
            AbilityLoadout = new AbilityDefinition[]
            {
                AbilityDatabase.Instance.GetAbility("ability_fortify"),
                AbilityDatabase.Instance.GetAbility("ability_restoration")
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
        //Todo determine which loadouts are available based on player progress, for now just return all thats on by default

        return GetAllDefinitions().Where(def => def.UnlockedByDefault == true).ToList();
    }

    public string GetCurrentLoadoutDisplayName()
    {
        var activePreset = SaveService.Instance.Current.Loadouts.Presets.Find(p => p.Id == CurrentLoadoutId);
        return activePreset != null ? activePreset.DisplayName : "Unknown Loadout";
    }
    public List<LoadoutDefinition> GetAllDefinitions() => definitionMap.Values.ToList();
    public LoadoutDefinition GetDefinition(string id) => id == null ? null : definitionMap[id];
}
