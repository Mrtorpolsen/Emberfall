// The account popup, opened by tapping the username in the top bar. For now it only offers deleting the account.
public static class AccountPopup
{
    public static void Open()
    {
        PopupManager.Instance.OpenPopup_ImgDescBtn(
            heading: "Account",
            description: UserProfile.Instance.userName,
            buttonDefinition: new PopupButtonDefinition
            {
                BtnText = "Delete account",
                LabelText = "Removes your progress",
                OnClick = OpenDeleteConfirmation
            });
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
