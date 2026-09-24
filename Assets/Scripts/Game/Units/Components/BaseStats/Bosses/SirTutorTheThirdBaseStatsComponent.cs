using UnityEngine;

public class SirTutorTheThirdBaseStatsComponent : BaseUnitStats
{
    //Only for tutor to end the tutorial
    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameManager.Instance.SetTutorialOver();
    }
}
