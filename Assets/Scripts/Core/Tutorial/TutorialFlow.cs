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
        if (hasInitialized)
            return;

        tutorialPresenter = new TutorialPresenter();

        await tutorialPresenter.Initialize(root);

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
                overrideOnClick = true
            },
            new TutorialStep
            {
                target = "",
                popup = new TutorialPopupData
                {
                    heading = "Units",
                    description = "",
                    hasButton = false
                },
                onComplete = () => {},
                overrideOnClick = true
            }
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
            PlayStep(2);
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