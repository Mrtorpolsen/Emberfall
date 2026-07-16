using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static LoadoutService;

public class LoadoutUIController : IUIScreenController
{
    private LoadoutView view;

    private LoadoutInteractionMode interactionMode = LoadoutInteractionMode.Viewing;

    private ActiveLoadout workingLoadout;

    private DefinitionCategory? selectedSlotType;
    private int selectedSlotIndex = -1;

    private bool isDirty = false;

    public void Initialize(IUIScreenView screenView)
    {
        if (screenView is not LoadoutView loadoutView)
        {
            Debug.LogError("ResearchUIController recieved the wrong view type");
            return;
        }

        view = loadoutView;

        workingLoadout = LoadoutService.Instance.CurrentLoadout.Clone();

        view.RenderLoadouts(BuildLoadoutSlots());
        view.RenderLoadoutCards(BuildLoadoutCards());

        DisplayLoadoutName();
    }

    private List<LoadoutSlotViewModel> BuildLoadoutSlots()
    {
        var loadoutSlots = new List<LoadoutSlotViewModel>();

        foreach (var loadout in workingLoadout.EnumerateSlots())
        {
            loadoutSlots.Add(BuildLoadoutSlots(loadout));
        }

        return loadoutSlots;
    }

    private LoadoutSlotViewModel BuildLoadoutSlots(LoadoutSlot loadoutSlot)
    {
        var slot = new LoadoutSlotViewModel();

        slot.isSelected = selectedSlotType == loadoutSlot.SlotType &&
            selectedSlotIndex == loadoutSlot.Index;

        slot.SlotType = loadoutSlot.SlotType;

        slot.isEmpty = loadoutSlot.Definition == null;

        slot.onClick = () =>
        {
            if (interactionMode == LoadoutInteractionMode.Viewing)
            {
                BeginSlotSelection(loadoutSlot);
            }
            else if (interactionMode == LoadoutInteractionMode.SelectingReplacement)
            {
                if (selectedSlotType == loadoutSlot.SlotType && selectedSlotIndex == loadoutSlot.Index)
                {
                    ClearSlot(loadoutSlot);
                }
                interactionMode = LoadoutInteractionMode.Viewing;
                ExitReplacementMode();
            }
        };

        if (!slot.isEmpty)
        {
            slot.label = loadoutSlot.Definition.DisplayName;
            slot.icon = loadoutSlot.Definition.Icon;

            slot.onLongPress = () =>
            {
                var description = "";

                if (loadoutSlot.SlotType == DefinitionCategory.Tower || loadoutSlot.SlotType == DefinitionCategory.Unit)
                {
                    var unitKey = LoadoutDatabase.Instance.GetSpawn(loadoutSlot.Definition.Id).Stats.name.ToLowerInvariant();

                    UnitStatsManager.Instance.ReloadPlayerData();
                    UnitStatsManager.Instance.CalculateFinalStatsByKey(unitKey);

                    var stats = UnitStatsManager.Instance.FinalStatsByUnit[unitKey];

                    description = BuildUnitStatsDescription(loadoutSlot.Definition, stats);
                }
                else if (loadoutSlot.SlotType == DefinitionCategory.Utility)
                {
                    description = BuildAbilityDescription(loadoutSlot.Definition);
                }

                PopupManager.Instance.OpenPopup(loadoutSlot.Definition.Icon.AssetGUID,
                    loadoutSlot.Definition.DisplayName,
                    description);
            };
        }

        return slot;
    }

    private List<LoadoutCardViewModel> BuildLoadoutCards()
    {
        var loadoutCards = new List<LoadoutCardViewModel>();
        foreach (var loadout in LoadoutService.Instance.GetAllAvailableLoadoutDefinitions())
        {
            loadoutCards.Add(BuildLoadoutCards(loadout));
        }
        return loadoutCards;
    }

    private LoadoutCardViewModel BuildLoadoutCards(LoadoutDefinition loadoutDefinition)
    {
        var card = new LoadoutCardViewModel();
        card.label = loadoutDefinition.DisplayName;
        card.icon = loadoutDefinition.Icon;
        card.Type = loadoutDefinition.SlotType;
        card.isSelectable = IsCardSelectable(loadoutDefinition);

        card.onClick = () =>
        {
            if (interactionMode == LoadoutInteractionMode.SelectingReplacement)
            {
                TryAssignCard(loadoutDefinition);
            }
            else if (interactionMode == LoadoutInteractionMode.Viewing)
            {
                Debug.Log($"Viewing loadout card: {loadoutDefinition.DisplayName}");
            }
        };

        card.onLongPress = () =>
        {
            var description = "";

            if (loadoutDefinition.SlotType == DefinitionCategory.Tower || loadoutDefinition.SlotType == DefinitionCategory.Unit)
            {
                var stats = LoadoutDatabase.Instance.GetSpawn(loadoutDefinition.Id).Stats;
                if (stats == null)
                {
                    Debug.LogError("Stats were null for " + loadoutDefinition.DisplayName);
                    return;
                }
                description = BuildUnitStatsDescription(loadoutDefinition, stats);
            }
            else
            {
                description = BuildAbilityDescription(loadoutDefinition);
            }

            PopupManager.Instance.OpenPopup(loadoutDefinition.Icon.AssetGUID,
                loadoutDefinition.DisplayName,
                description);
        };

        return card;
    }

    private void DisplayLoadoutName()
    {
        view.SetLoadoutHeading(LoadoutService.Instance.GetCurrentLoadoutDisplayName());
    }

    private void BeginSlotSelection(LoadoutSlot slot)
    {
        ClearSelection();

        selectedSlotType = slot.SlotType;
        selectedSlotIndex = slot.Index;

        view.ShowCardContainer(slot.SlotType);

        SetInteractionMode(LoadoutInteractionMode.SelectingReplacement);

        RefreshSlotState();
        Debug.Log($"Selected slot for replacement: {slot.Definition?.DisplayName ?? "Empty Slot"} of type {slot.SlotType}");
    }

    private void TryAssignCard(LoadoutDefinition definition)
    {
        if (selectedSlotType == null)
            return;

        if (IsEquipped(definition.Id))
            return;

        var previousDefinition = GetDefinition(
            selectedSlotType.Value,
            selectedSlotIndex
        );

        SetDefinition(selectedSlotType.Value, selectedSlotIndex, definition);

        selectedSlotType = null;
        selectedSlotIndex = -1;

        ExitReplacementMode();
    }

    private void ClearSlot(LoadoutSlot loadoutSlot)
    {
        SetDefinition(loadoutSlot.SlotType, loadoutSlot.Index, null);
        ClearSelection();
        SetInteractionMode(LoadoutInteractionMode.Viewing);
        RefreshSlotState();
    }

    private LoadoutDefinition GetDefinition(DefinitionCategory type, int index)
    {
        return type switch
        {
            DefinitionCategory.Unit =>
                workingLoadout.UnitLoadout[index],

            DefinitionCategory.Tower =>
                workingLoadout.TowerLoadout[index],

            DefinitionCategory.Utility =>
                workingLoadout.AbilityLoadout[index],

            _ => null
        };
    }

    private void SetDefinition(DefinitionCategory type, int index, LoadoutDefinition definition)
    {
        switch (type)
        {
            case DefinitionCategory.Unit:
                workingLoadout.UnitLoadout[index] =
                    (SpawnDefinition)definition;
                break;

            case DefinitionCategory.Tower:
                workingLoadout.TowerLoadout[index] =
                    (SpawnDefinition)definition;
                break;

            case DefinitionCategory.Utility:
                workingLoadout.AbilityLoadout[index] =
                    (AbilityDefinition)definition;
                break;
        }
        isDirty = true;
    }

    private void ExitReplacementMode()
    {
        ClearSelection();

        RefreshSlotState();

        SetInteractionMode(LoadoutInteractionMode.Viewing);
    }

    private void SetInteractionMode(LoadoutInteractionMode mode)
    {
        interactionMode = mode;

        RefreshCardState();
    }

    private void RefreshCardState()
    {
        view.RenderLoadoutCards(BuildLoadoutCards());
    }

    private void RefreshSlotState()
    {
        view.RenderLoadouts(BuildLoadoutSlots());
    }

    private void ClearSelection()
    {
        selectedSlotType = null;
        selectedSlotIndex = -1;
    }

    private bool IsEquipped(string id)
    {
        return workingLoadout.EnumerateSlots()
            .Any(s => s.Definition?.Id == id);
    }

    private bool IsCardSelectable(LoadoutDefinition def)
    {
        if (interactionMode == LoadoutInteractionMode.Viewing)
            return true;

        if (selectedSlotType == null)
            return false;

        if (def.SlotType != selectedSlotType.Value)
            return false;

        if (IsEquipped(def.Id))
            return false;

        return true;
    }

    public async Task SaveLoadout()
    {
        await LoadoutService.Instance.SaveLoadout(workingLoadout);
    }

    private async Task SaveLoadoutSafe()
    {
        try
        {
            await SaveLoadout();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            isDirty = true;
        }
    }

    private string BuildUnitStatsDescription(LoadoutDefinition loadoutDefinition, UnitStatsDefinition stats)
    {
        string description = "";

        switch (loadoutDefinition.SlotType)
        {
            case DefinitionCategory.Tower:
                description = "Cost: " + stats.cost + "\n" +
                    "Damage: " + stats.attackDamage + "\n" +
                    "Attack Speed: " + stats.attackSpeed + "\n" +
                    "Attack Range: " + stats.attackRange;
                break;

            case DefinitionCategory.Unit:
                description = "Cost: " + stats.cost + "\n" +
                    "Health: " + stats.maxHealth + "\n" +
                    "Armor: " + stats.armor + "\n" +
                    "Damage: " + stats.attackDamage + "\n" +
                    "Attack Speed: " + stats.attackSpeed + "\n" +
                    "Attack Range: " + stats.attackRange + "\n" +
                    "Crit Damage: " + stats.critDamage + "\n" +
                    "Mass: " + stats.mass;
                break;
        }

        return description;
    }
    private string BuildUnitStatsDescription(LoadoutDefinition loadoutDefinition, FinalStats stats)
    {
        string description = "";

        switch (loadoutDefinition.SlotType)
        {
            case DefinitionCategory.Tower:
                description = "Cost: " + stats.cost + "\n" +
                    "Damage: " + stats.attackDamage + "\n" +
                    "Attack Speed: " + stats.attackSpeed + "\n" +
                    "Attack Range: " + stats.attackRange;
                break;

            case DefinitionCategory.Unit:
                description = "Cost: " + stats.cost + "\n" +
                    "Health: " + stats.maxHealth + "\n" +
                    "Armor: " + stats.armor + "\n" +
                    "Damage: " + stats.attackDamage + "\n" +
                    "Attack Speed: " + stats.attackSpeed + "\n" +
                    "Attack Range: " + stats.attackRange + "\n" +
                    "Crit Chance: " + stats.critChance + "\n" +
                    "Crit Damage: " + stats.critDamage + "\n" +
                    "Mass: " + stats.mass;
                break;
        }

        return description;
    }

    private string BuildAbilityDescription(LoadoutDefinition loadoutDefinition)
    {
        var def = LoadoutDatabase.Instance.GetAbility(loadoutDefinition.Id);
        var stats = ResearchService.Instance.playerResearchTree.GetResearchById(loadoutDefinition.UnlockId);

        return $"Cooldown: {def.cooldown}s\nCost: {def.Cost}\n{stats.Description}";
    }

    public void Cleanup()
    {
        if (isDirty)
        {
            _ = SaveLoadoutSafe();
            isDirty = false; // optimistic reset
        }
    }
}