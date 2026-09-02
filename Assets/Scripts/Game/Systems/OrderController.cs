using UnityEngine;

public class OrderController : MonoBehaviour
{
    public void MeleeAdvance()
    {
        GameManager.Instance.AdvanceMeleeRally();
    }

    public void MeleeFallback()
    {
        GameManager.Instance.FallbackMeleeRally();
    }

    public void RangedAdvance()
    {
        GameManager.Instance.AdvanceRangedRally();
    }

    public void RangedFallback()
    {
        GameManager.Instance.FallbackRangedRally();
    }

    public void AllAdvance()
    {
        MeleeAdvance();
        RangedAdvance();
    }

    public void AllFallback()
    {
        MeleeFallback();
        RangedFallback();
    }
}