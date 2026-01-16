using System.Collections;
using UnityEngine;

public static class CoroutineHelpers
{
    public static IEnumerator DoAfterDelay(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
}
