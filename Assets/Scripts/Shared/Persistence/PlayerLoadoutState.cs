using System;
using System.Collections.Generic;

[Serializable]
public class PlayerLoadoutState
{
    public string[] UnitLoadout = new string[4];
    public string[] TowerLoadout = new string[3];
    public string[] AbilityLoadout = new string[2];
}

[Serializable]
public class SavedLoadout
{
    public int Id;
    public string DisplayName;
    public PlayerLoadoutState State;
}

[Serializable]
public class PlayerLoadoutCollection
{
    public List<SavedLoadout> Presets = new();
    public int ActiveLoadoutId;
}