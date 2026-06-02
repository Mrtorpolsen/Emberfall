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
            slot.Type = loadoutSlot.Type;
        }

        //TODO Build on click and long click

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
        //TODO locked and Build on click and long click
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