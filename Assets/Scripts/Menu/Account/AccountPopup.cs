// The account popup, opened by tapping the username in the top bar: link Play Games (guests on Android) and delete the account.
public static class AccountPopup
{
    public static void Open()
    {
        var deleteButton = new PopupButtonDefinition
        {
            BtnText = "Delete account",
            LabelText = "Removes your progress",
            OnClick = OpenDeleteConfirmation
        };

        bool isGuest = AccountService.IsGuest;
        string description = isGuest
            ? UserProfile.Instance.userName + "\n\nGuest account: your progress is only saved on this device."
            : UserProfile.Instance.userName;

        if (!isGuest || !PlayGamesManager.IsSupported)
        {
            PopupManager.Instance.OpenPopup_ImgDescBtn(
                heading: "Account",
                description: description,
                buttonDefinition: deleteButton);
            return;
        }

        PopupManager.Instance.OpenPopup_ImgDescBtn(
            heading: "Account",
            description: description,
            buttonDefinition: new PopupButtonDefinition
            {
                BtnText = "Link Play Games",
                LabelText = "Keep your progress safe",
                OnClick = LinkPlayGames
            },
            secondaryButtonDefinition: deleteButton);
    }

    private static async void LinkPlayGames()
    {
        if (!UserProfile.Instance.IsLoggedIn())
        {
            Toast.Show("Connect to the internet to link Play Games.");
            return;
        }

        LoadingSpinner.Instance.ShowSpinner();
        LinkResult result;

        try
        {
            result = await AccountService.LinkPlayGamesAsync();
        }
        finally
        {
            LoadingSpinner.Instance.HideSpinner();
        }

        switch (result)
        {
            case LinkResult.Linked:
                PopupManager.Instance.ClosePopup();
                Toast.Show("Play Games linked. Your progress is now tied to your Google account.");
                break;
            case LinkResult.AlreadyLinkedElsewhere:
                Toast.Show("That Play Games account belongs to another player. You're still on this device's account.");
                break;
            case LinkResult.Cancelled:
                Toast.Show("Couldn't sign in to Play Games.");
                break;
            default:
                Toast.Show("Couldn't link Play Games. Try again.");
                break;
        }
    }

    private static void OpenDeleteConfirmation()
    {
        PopupManager.Instance.OpenPopup_ImgDescBtn(
            heading: "Delete account?",
            description: "This permanently deletes your account and all your progress.",
            buttonDefinition: new PopupButtonDefinition
            {
                BtnText = "Delete",
                LabelText = "This can't be undone",
                OnClick = DeleteAccount
            });
    }

    private static async void DeleteAccount()
    {
        if (!UserProfile.Instance.IsLoggedIn())
        {
            Toast.Show("Connect to the internet to delete your account.");
            return;
        }

        PopupManager.Instance.ButtonIsActive(false);
        LoadingSpinner.Instance.ShowSpinner();

        if (await AccountService.DeleteAccountAsync())
        {
            Toast.ShowOnNextScreen("Your account was deleted.");
            AccountService.RestartGame();
            return;
        }

        LoadingSpinner.Instance.HideSpinner();
        PopupManager.Instance.ButtonIsActive(true);
        Toast.Show("Couldn't delete your account. Try again.");
    }
}
