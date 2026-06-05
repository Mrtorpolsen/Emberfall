using System.Collections.Generic;
using UnityEngine;

public class LoadoutUIController : IUIScreenController
{
    private LoadoutView view;
    public void Initialize(IUIScreenView screenView)
    {
        if (screenView is not LoadoutView loadoutView)
        {
            Debug.LogError("ResearchUIController recieved the wrong view type");
            return;
        }

        view = loadoutView;

        BuildLoadoutSlots();

        BuildLoadoutCards();

        view.RenderLoadouts(BuildLoadoutSlots());
        view.RenderLoadoutCards(BuildLoadoutCards());

        DisplayLoadoutName();
    }

    private List<LoadoutSlotViewModel> BuildLoadoutSlots()
    {
        var loadoutSlots = new List<LoadoutSlotViewModel>();

        foreach (var loadout in LoadoutService.Instance.CurrentLoadout.EnumerateSlots())
        {
            loadoutSlots.Add(BuildLoadoutSlots(loadout));
        }

        return loadoutSlots;
    }

    private LoadoutSlotViewModel BuildLoadoutSlots(LoadoutSlot loadoutSlot)
    {
        var slot = new LoadoutSlotViewModel();
        
        slot.isEmpty = loadoutSlot.Definition == null;

        if(!slot.isEmpty)
        {
            slot.label = loadoutSlot.Definition.DisplayName;
            slot.icon = loadoutSlot.Definition.Icon;
            slot.SlotType = loadoutSlot.SlotType;
        }

        //TODO Build on click and long click
        slot.onLongPress = () =>
        {
            var description = "";

            if(loadoutSlot.SlotType == DefinitionCategory.Tower || loadoutSlot.SlotType == DefinitionCategory.Unit)
            {
                var stats = LoadoutDatabase.Instance.GetSpawn(loadoutSlot.Definition.Id).Stats;
                if(stats == null)
                {
                    Debug.LogError("Stats were null for " + loadoutSlot.Definition.DisplayName);
                    return;
                }
                if(loadoutSlot.SlotType == DefinitionCategory.Tower)
                {
                    description = "Cost: " + stats.cost + "\n" +
                    "Damage: " + stats.attackDamage + "\n" +
                    "Attack Speed: " + stats.attackSpeed + "\n" +
                    "Attack Range: " + stats.attackRange;
                }
                else
                {
                    description = "Cost: " + stats.cost + "\n" +
                    "Health: " + stats.maxHealth + "\n" +
                    "Armor: " + stats.armor + "\n" +
                    "Damage: " + stats.attackDamage + "\n" +
                    "Attack Speed: " + stats.attackSpeed + "\n" +
                    "Attack Range: " + stats.attackRange + "\n" +
                    "Crit Chance: " + stats.critChance + "\n" + 
                    "Crit Damage: " + stats.critDamage;
                }
            }
            else
            {
                //Look into making maybe a getDescription method on the ResearchDefinition that way we can encapsulate the description building there
                var stats = ResearchService.Instance.playerResearchTree.GetResearchById(loadoutSlot.Definition.Id);
                description = $"{stats.Description}";
            }

            PopupManager.Instance.OpenPopup(loadoutSlot.Definition.Icon.AssetGUID, 
                loadoutSlot.Definition.DisplayName,
                description);
        };

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
        //TODO locked and Build on click and long click

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
                if (loadoutDefinition.SlotType == DefinitionCategory.Tower)
                {
                    description = "Cost: " + stats.cost + "\n" +
                    "Damage: " + stats.attackDamage + "\n" +
                    "Attack Speed: " + stats.attackSpeed + "\n" +
                    "Attack Range: " + stats.attackRange;
                }
                else
                {
                    description = "Cost: " + stats.cost + "\n" +
                    "Health: " + stats.maxHealth + "\n" +
                    "Armor: " + stats.armor + "\n" +
                    "Damage: " + stats.attackDamage + "\n" +
                    "Attack Speed: " + stats.attackSpeed + "\n" +
                    "Attack Range: " + stats.attackRange + "\n" +
                    "Crit Chance: " + stats.critChance + "\n" +
                    "Crit Damage: " + stats.critDamage;
                }
            }
            else
            {
                //Look into making maybe a getDescription method on the ResearchDefinition that way we can encapsulate the description building there
                var def = LoadoutDatabase.Instance.GetAbility(loadoutDefinition.Id);
                var stats = ResearchService.Instance.playerResearchTree.GetResearchById(loadoutDefinition.UnlockId);

                description = $"Cooldown: {def.cooldown}s\nCost: {def.Cost}\n{stats.Description}";
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


    public void Cleanup()
    {
        Debug.Log("TODO Cleaning up LoadoutUIController");
    }
}