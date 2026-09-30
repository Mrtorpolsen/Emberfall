using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TutorialFlow : MonoBehaviour
{
    public static TutorialFlow Instance { get; private set; }

    private TutorialPresenter tutorialPresenter;

    public int currentStep = 0;

    private TutorialStep[] steps;

    public bool hasInitialized = false;

    public bool isPlayingTutorial = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task Initialize(VisualElement root)
    {
        //Inject the VTA
        tutorialPresenter = new TutorialPresenter();
        await tutorialPresenter.Initialize(root);

        if (hasInitialized)
            return;

        steps = new TutorialStep[]
        {
            new TutorialStep
            {
                popup = new TutorialPopupData
                {
                    heading = "Welcome to Emberfall!",
                    description = $"{UserProfile.Instance.userName}, never heard that before. Welcome to emberfall! Emberfall is a td-ish game where you defend against the enemy, earn currency and spend that currency to upgrade your units and unlock new ones, rising through the difficulties until you reach the top! But enough talk, let's get going!",
                    hasButton = true
                },
                onComplete = () =>
                {
                    currentStep++;
                    PlayStep(currentStep);
                }
            },

            new TutorialStep
            {
                target = "Btn_Play",
                popup = new TutorialPopupData
                {
                    heading = "Let's go!",
                    description = "Usually when you press play you need to select a difficulty, but since this is a tutorial I've taken care of it for you!\nGood luck!",
                    hasButton = false
                },
                onComplete = () =>
                {
                    GameSettingsService.Instance.SetDifficulty(DifficultyLevel.Tutorial);
                    SceneManager.LoadScene("Game");
                    currentStep++;
                },
                overrideOnClick = true
            },
            new TutorialStep
            {
                target = "CinderContainer",
                popup = new TutorialPopupData
                {
                    heading = "Currency",
                    description = "Here in Emberfall we run on cinders, its what makes upgrade and unlocks of new units possible. After the last battle you got 2000 of them, let's go spend them!",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "Btn_Armory",
                popup = new TutorialPopupData
                {
                    heading = "The Armory",
                    description = "In the Armory is where you find your talents and your loadout. Let's head in there!",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "Btn_ForgeNav",
                popup = new TutorialPopupData
                {
                    heading = "The Forge",
                    description = "The Forge is where you upgrade your units and unlock new ones by spending the currency you earn. Let's take a look!",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayStep(currentStep);
                },
            },
            new TutorialStep
            {
                popup = new TutorialPopupData
                {
                    heading = "Units",
                    description = "In here are the talents of all your unlocked units. As you progress in game and talents, you will unlock more. They will automatically appear in here.",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "Btn_Upg",
                popup = new TutorialPopupData
                {
                    heading = "Upgrade a Unit",
                    description = "Now lets upgrade a unit to make it stronger! Press here to access the Fighters talents.",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayStep(currentStep);
                },
            },
            new TutorialStep
            {
                popup = new TutorialPopupData
                {
                    heading = "Talents",
                    description = "In here you see all the fighter specific talents. In here you will be working your way from the top down, gradually unlocking stronger and more expensive talents. Some units even unlock new ones near the bottom!",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "TalentNodeContainer",
                popup = new TutorialPopupData
                {
                    heading = "Purchasing a talents",
                    description = "Now click the T1 talent of the fighter, and lets get to upgrading!",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayStep(currentStep);
                },
            },
            new TutorialStep
            {
                popup = new TutorialPopupData
                {
                    heading = "Talent details",
                    description = "In here you see all there is to know about the talent. What it's effect is, whats required to unlock it, and how much it costs (this can also be seen in the talent overview). You can look at the later!",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "Btn_CTA",
                popup = new TutorialPopupData
                {
                    heading = "Buy the talent",
                    description = "Click here to buy the talent",
                    hasButton = false
                },
                onComplete = () => {
                    PopupManager.Instance.ClosePopup();
                    currentStep++;
                    PlayStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "TalentNodeContainer",
                popup = new TutorialPopupData
                {
                    heading = "Keeping track",
                    description = "You can keep track of what you have purchased and how much they each cost out here.",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "Btn_Armory",
                popup = new TutorialPopupData
                {
                    heading = "Back to the Armory",
                    description = "Now that you have bought a talent, lets head on back to the forge and check out the loadout.",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "Btn_LoadoutNav",
                popup = new TutorialPopupData
                {
                    heading = "The Loadout Menu",
                    description = "The loadout menu is where you select and experiment with the units, abilities and towers that you bring into battle! In there you can also see the base stats and the stats with your upgrades applied, of all you have unlocked.",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "SelectedLoutoutContainer",
                popup = new TutorialPopupData
                {
                    heading = "Your selected loadout",
                    description = "Here is your current loadout, which is what you will bring into battle",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "TabMenu",
                popup = new TutorialPopupData
                {
                    heading = "Different Categories",
                    description = "There are three different categories, each having a limited number you can bring into battle. From the top, Abilities, Tower and last Units.",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "unity-content-viewport",
                popup = new TutorialPopupData
                {
                    heading = "The available",
                    description = "Here is what you have currrently unlocked and can bring. More will appear as you unlock them",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "LoadoutSlotUnit0",
                popup = new TutorialPopupData
                {
                    heading = "Switching Units",
                    description = "Click here on the empty slot to enter picking mode.",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "LoadoutCard",
                popup = new TutorialPopupData
                {
                    heading = "Switching Units",
                    description = "Now click on the Cavalier to bring it to your loadout.",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "LoadoutSlotUnit1",
                popup = new TutorialPopupData
                {
                    heading = "Your Stats",
                    description = "Press and hold to open your stats",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayStep(currentStep);
                },
            },
            /* 
            new TutorialStep
            {
                target = "",
                popup = new TutorialPopupData
                {
                    heading = "",
                    description = "",
                    hasButton = false
                },
                onComplete = () => {},
                overrideOnClick = true
            }
             */
        };

        hasInitialized = true;

        if (!SaveService.Instance.Current.Flags.HasCompletedTutorial)
        {
            isPlayingTutorial = true;
        }
    }

    public void MainMenuReady()
    {
        if (isPlayingTutorial)
        {
            LoadoutService.Instance.SetTutorialLoadout();
            PlayStep(2);
            //PlayStep(currentStep);
        }
    }

    public void ArmoryReady()
    {
        if (isPlayingTutorial)
        {
            //LoadoutService.Instance.SetTutorialLoadout();
            //PlayStep(12);
            //PlayStep(currentStep);
        }
    }

    public void PlayStep(int step)
    {
        if (step >= steps.Length)
        {
            // TutorialCompleted();
            isPlayingTutorial = false;
            return;
        }

        currentStep = step;
        if (currentStep == (steps.Count() - 3))
        {
            Debug.Log("hey");
        }

        TutorialStep tutorialStep = steps[currentStep];

        tutorialPresenter.ExecuteTutorialStep(
            tutorialStep.popup,
            tutorialStep
        );
    }

    private void TutorialCompleted()
    {
        SaveService.Instance.Current.Flags.HasCompletedTutorial = true;
        SaveService.Instance.Save();
    }
}