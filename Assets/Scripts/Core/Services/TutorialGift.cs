using System.Threading.Tasks;

// The Cinders for finishing the gameplay tutorial, granted by the server once per player (ClaimTutorialGift).
// Claimed when the gameplay tutorial ends; if that happened offline, at the next launch.
public static class TutorialGift
{
    public static async Task ClaimIfDueAsync()
    {
        var save = SaveService.Instance != null ? SaveService.Instance.Current : null;

        if (save == null || save.Progress.HasReceivedLoginGift || !ServerActions.IsOnline)
            return;

        await ServerActions.SendAsync(module => module.ClaimTutorialGift());
    }
}
