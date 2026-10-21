using System;

// One float per StatType, indexed by the enum value. StatType must stay contiguous from 0.
public class StatBlock
{
    private static readonly int StatCount = Enum.GetValues(typeof(StatType)).Length;

    private readonly float[] values = new float[StatCount];

    public float Get(StatType stat) => values[(int)stat];

    public void Set(StatType stat, float value) => values[(int)stat] = value;

    public void Add(StatType stat, float delta) => values[(int)stat] += delta;

    public StatBlock Clone()
    {
        var clone = new StatBlock();
        Array.Copy(values, clone.values, StatCount);
        return clone;
    }
}
