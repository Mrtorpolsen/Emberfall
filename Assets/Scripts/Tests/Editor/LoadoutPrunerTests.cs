using NUnit.Framework;
using System.Collections.Generic;

public class LoadoutPrunerTests
{
    private static PlayerLoadoutState CreateState()
    {
        return new PlayerLoadoutState
        {
            UnitLoadout = new[] { "spawn_fighter", "spawn_berserker", null, "spawn_ranger" },
            TowerLoadout = new[] { "spawn_tower", null, "spawn_bomb" },
            AbilityLoadout = new[] { "ability_fortify", "ability_restoration" }
        };
    }

    [Test]
    public void RemoveUnavailable_EmptiesOnlyUnavailableSlots()
    {
        var unavailable = new HashSet<string> { "spawn_berserker", "spawn_bomb", "ability_restoration" };
        PlayerLoadoutState state = CreateState();

        bool removed = LoadoutPruner.RemoveUnavailable(state, id => !unavailable.Contains(id));

        Assert.IsTrue(removed);
        CollectionAssert.AreEqual(new[] { "spawn_fighter", null, null, "spawn_ranger" }, state.UnitLoadout);
        CollectionAssert.AreEqual(new[] { "spawn_tower", null, null }, state.TowerLoadout);
        CollectionAssert.AreEqual(new[] { "ability_fortify", null }, state.AbilityLoadout);
    }

    [Test]
    public void RemoveUnavailable_AllAvailable_ChangesNothing()
    {
        PlayerLoadoutState state = CreateState();
        PlayerLoadoutState expected = CreateState();

        bool removed = LoadoutPruner.RemoveUnavailable(state, _ => true);

        Assert.IsFalse(removed);
        CollectionAssert.AreEqual(expected.UnitLoadout, state.UnitLoadout);
        CollectionAssert.AreEqual(expected.TowerLoadout, state.TowerLoadout);
        CollectionAssert.AreEqual(expected.AbilityLoadout, state.AbilityLoadout);
    }

    [Test]
    public void RemoveUnavailable_NeverAsksAboutEmptySlots()
    {
        PlayerLoadoutState state = CreateState();

        LoadoutPruner.RemoveUnavailable(state, id =>
        {
            Assert.IsNotNull(id);
            return true;
        });
    }

    [Test]
    public void RemoveUnavailable_HandlesMissingArrays()
    {
        var state = new PlayerLoadoutState { UnitLoadout = null, TowerLoadout = null, AbilityLoadout = null };

        Assert.IsFalse(LoadoutPruner.RemoveUnavailable(state, _ => false));
        Assert.IsFalse(LoadoutPruner.RemoveUnavailable(null, _ => false));
    }
}
