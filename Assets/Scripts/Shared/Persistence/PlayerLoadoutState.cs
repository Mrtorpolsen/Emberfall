using System;

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
    public string Id;
    public string DisplayName;
    public PlayerLoadoutState State;
}
