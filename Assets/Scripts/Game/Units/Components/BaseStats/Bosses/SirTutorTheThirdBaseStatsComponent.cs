using UnityEngine;

public class SirTutorTheThirdBaseStatsComponent : BaseUnitStats
{
    // Killing the tutor ends the tutorial; leaving the match early doesn't
    protected override void OnDeath()
    {
        GameManager.Instance.SetTutorialOver();
    }
}
