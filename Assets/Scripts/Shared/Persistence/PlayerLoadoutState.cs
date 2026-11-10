using System.Collections.Generic;

public class PlayerLoadoutState
{
    public string[] UnitLoadout = new string[4];
    public string[] TowerLoadout = new string[3];
    public string[] AbilityLoadout = new string[2];
}

public class SavedLoadout
{
    public int Id;
    public string DisplayName;
    public PlayerLoadoutState State;
}

public class PlayerLoadoutCollection
{
    public List<SavedLoadout> Presets = new();
    public int ActiveLoadoutId;
    public int NextLoadoutId = 1;
}