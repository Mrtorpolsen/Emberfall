using UnityEngine;

[CreateAssetMenu(fileName = "PrefabDatabase", menuName = "Game/PrefabDatabase")]
public class PrefabDatabase : ScriptableObject
{
    public GameObject fighterPrefab;
    public GameObject rangerPrefab;
    public GameObject cavalierPrefab;
    public GameObject gatePrefab;
    public GameObject towerPrefab;
    public GameObject giantPrefab;
    public GameObject eliteFighterPrefab;
}
