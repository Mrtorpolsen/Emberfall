using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITargetingAgent
{
    IReadOnlyList<ThreatLevel> PreferredPriorities { get; }
}