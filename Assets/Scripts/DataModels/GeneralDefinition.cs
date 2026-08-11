using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/General Definition")]
public class GeneralDefinition : ScriptableObject
{
    public string id;
    public string generalName;
    public List<string> tauntPhrases;
    public List<SpawnDefinition> unitRoster;
    public SpawnDefinition generalUnit;
}