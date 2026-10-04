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

    public bool isBlockingLoadoutClear = false;

    private int preGamePlaySteps = 2;

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
                    description = $"{UserProfile.Instance.userName}, never heard that before. Welcome to emberfall!\n\nEmberfall is a td-ish game where you defend against the enemy, earn currency and spend that currency to upgrade your units and unlock new ones, rising through the difficulties until you reach the top!\n\nBut enough talk, let's get going!",
                    hasButton = true
                },
                onComplete = () =>
                {
                    currentStep++;
                    PlayMenuStep(currentStep);
                }
            },
            new TutorialStep
            {
                target = "Btn_Play",
                popup = new TutorialPopupData
                {
                    heading = "Let's go!",
                    description = "Usually when you press play you need to select a difficulty, but since this is a tutorial I've taken care of it for you!\n\nGood luck!",
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
                    description = "Here in Emberfall we run on cinders, its what makes upgrades and unlocking new units possible. After the last battle you got 2000 of them, let's go spend them!",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "Btn_Armory",
                popup = new TutorialPopupData
                {
                    heading = "The Armory",
                    description = "The Armory is where you find your talents and your loadout. Let's head in there!",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "Btn_ForgeNav",
                popup = new TutorialPopupData
                {
                    heading = "The Forge",
                    description = "The Forge is where you upgrade your units and unlock new ones by spending the currency you earn.",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                popup = new TutorialPopupData
                {
                    heading = "Units",
                    description = "In here are the talents of all your unlocked units. As you progress through the talents you will unlock more. They will automatically appear in here as you unlock them.",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "Btn_Refund_Talents",
                popup = new TutorialPopupData
                {
                    heading = "Refunds",
                    description = "And dont worry, if you regret your talent choices or want to try something else, you can refund them here!",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "Btn_Upg",
                popup = new TutorialPopupData
                {
                    heading = "Upgrade a Unit",
                    description = "Now lets upgrade a unit to make it stronger!\n\nPress here to access the Fighters talents.",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    TalentService.Instance.SetTutorialTalentState();
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                popup = new TutorialPopupData
                {
                    heading = "Talents",
                    description = "In here you'll see all the fighter specific talents. You'll be working your way from the top down, gradually unlocking stronger and more expensive talents. Some units even unlock new ones near the bottom!",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "TalentNodeContainer",
                popup = new TutorialPopupData
                {
                    heading = "Purchasing a talents",
                    description = "Now click the tier 1 talent, and let's get to upgrading!",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                popup = new TutorialPopupData
                {
                    heading = "Talent details",
                    description = "In here you'll see all there is to know about the talent. What it does, what is required to unlock it, and how much it'll cost (this can also be seen in the talent overview).",
                    hasButton = true,
                    position = TutorialPopupLocation.Top
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "Btn_CTA",
                popup = new TutorialPopupData
                {
                    heading = "Buy the talent",
                    description = "Click here to buy the talent.",
                    hasButton = false
                },
                onComplete = () => {
                    PopupManager.Instance.ClosePopup();
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "TalentNodeContainer",
                popup = new TutorialPopupData
                {
                    heading = "Keeping track",
                    description = "You can keep track of what you have purchased and how much they each cost.",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "Btn_Armory",
                popup = new TutorialPopupData
                {
                    heading = "Back to the Armory",
                    description = "Now that you have bought a talent, let's head on back to the forge and check out the loadout.",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "Btn_LoadoutNav",
                popup = new TutorialPopupData
                {
                    heading = "The Loadout Menu",
                    description = "The loadout menu is where you select and experiment with the units, abilities and towers that you bring into battle!\n\nIn there you can also see the base stats and the stats with your upgrades applied, of all you have unlocked.",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
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
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "TabMenu",
                popup = new TutorialPopupData
                {
                    heading = "Different Categories",
                    description = "There are three different categories, each having a limited number you can bring into battle.",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "unity-content-viewport",
                popup = new TutorialPopupData
                {
                    heading = "The available",
                    description = "Here is what you have currrently unlocked. More will appear as you unlock them",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "LoadoutSlotUnit0",
                popup = new TutorialPopupData
                {
                    heading = "Switching Units",
                    description = "Click on the empty slot to enter picking mode.",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "LoadoutCardUnit0",
                popup = new TutorialPopupData
                {
                    heading = "Switching Units",
                    description = "Click here to bring the Cavalier into your loadut.",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "UnitRowContainer",
                popup = new TutorialPopupData
                {
                    heading = "Good job!",
                    description = "These are the units you bring into battle, and in this order in the menu.",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                    isBlockingLoadoutClear = true;
                },
            },
            new TutorialStep
            {
                target = "LoadoutSlotUnit1",
                popup = new TutorialPopupData
                {
                    heading = "Your Stats",
                    description = "If you press and hold you can see the details of the unit / tower / ability. In the top its your stats, at the bottom is the base stats. For now press and hold here to see your fighter stats.",
                    hasButton = false,
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                    isBlockingLoadoutClear = false;
                },
                interaction = TutorialInteraction.LongClick
            },
            new TutorialStep
            {
                popup = new TutorialPopupData
                {
                    heading = "Your Stats",
                    description = "These are the stats of your unit with your upgrades applied. See how it has 21 damage? Thats from the talent we purchased earlier.",
                    hasButton = true,
                    position = TutorialPopupLocation.Top
                },
                onComplete = () => {
                    PopupManager.Instance.ClosePopup();
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "Btn_Research",
                popup = new TutorialPopupData
                {
                    heading = "Next up Research!",
                    description = "Now lets head to the research screen. Dont worry, we're almost there so you can get back to defending!",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    ResearchService.Instance.SetTutorialResearchState();
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                popup = new TutorialPopupData
                {
                    heading = "Different Research Categories",
                    description = "Here you see the different categories you can research. They all contain small upgrades that applies to all within that category! The only exception being Global Ability, which contains powerful and expensive abilities you can activate in battle!",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "Button_CategoryContainer",
                popup = new TutorialPopupData
                {
                    heading = "Purchasing a research for units",
                    description = "Lets buy one for units",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "ResearchNodeContainer",
                popup = new TutorialPopupData
                {
                    heading = "Research",
                    description = "Here you will find the details about the research, what stage it is, what it does and how long it takes to research it.",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "Button_PurchaseResearch",
                popup = new TutorialPopupData
                {
                    heading = "Start Research",
                    description = "Lets start a research and get more health on our units.",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "Btn_Research",
                popup = new TutorialPopupData
                {
                    heading = "Back to the overview",
                    description = "In the overview you can see your current active research.",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                popup = new TutorialPopupData
                {
                    heading = "Active Research",
                    description = "You can max have one active research per category.",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                target = "Btn_Leaderboard",
                popup = new TutorialPopupData
                {
                    heading = "Last step!",
                    description = "Last up is the leaderboard.",
                    hasButton = false
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
                },
            },
            new TutorialStep
            {
                popup = new TutorialPopupData
                {
                    heading = "Thats it!",
                    description = "In here you will be able to see how long you and your fellow players have survived.\n\n Now all that is left is to wish you good luck!\n\nGood luck!",
                    hasButton = true
                },
                onComplete = () => {
                    currentStep++;
                    PlayMenuStep(currentStep);
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
            PlayMenuStep(2);
            //StartTutorial();
        }
    }

    private void StartTutorial()
    {
        LoadoutService.Instance.SetTutorialLoadout();
        PlayMenuStep(currentStep);
    }

    public void PlayMenuStep(int step)
    {
        if (step >= steps.Length)
        {
            TutorialCompleted();
            isPlayingTutorial = false;
            return;
        }

        currentStep = step;

        TutorialStep tutorialStep = steps[currentStep];

        tutorialPresenter.ExecuteTutorialStep(
            tutorialStep.popup,
            tutorialStep
        );
    }

    public void RebindCurrentTarget()
    {
        tutorialPresenter?.RebindCurrentTarget();
    }

    public void SetGamePlayTutorialOver()
    {
        currentStep = preGamePlaySteps;
    }

    private void TutorialCompleted()
    {
        tutorialPresenter.Hide();
        SaveService.Instance.Current.Flags.HasCompletedTutorial = true;
        SaveService.Instance.Save();
    }
}
