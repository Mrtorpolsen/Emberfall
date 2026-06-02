using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

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

        view.RenderLoadouts(BuildLoadoutSlots());
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

    public void Cleanup()
    {
        Debug.Log("TODO Cleaning up LoadoutUIController");
    }
}