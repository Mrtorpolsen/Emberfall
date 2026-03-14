using UnityEngine;

public class UnitMetadata : MonoBehaviour
{
    [SerializeField] private Team team;
    [SerializeField] private bool isElite;

    public Team Team => team;
    public bool IsElite => isElite;
    public void SetTeam(Team newTeam)
    {
        //Consider just setter
        team = newTeam;
    }

}