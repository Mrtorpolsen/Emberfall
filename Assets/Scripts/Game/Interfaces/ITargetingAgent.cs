using System.Collections.Generic;

public interface ITargetingAgent
{
    IReadOnlyList<ThreatLevel> PreferredPriorities { get; }
}