using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TutorialFlow : MonoBehaviour
{
    public static TutorialFlow Instance {  get; private set; }

    private TutorialPresenter tutorialPresenter;

    private int currentStep = 0;

    private TutorialStep[] steps;

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
        tutorialPresenter = new TutorialPresenter();

        await tutorialPresenter.Initialize(root);

        steps = new TutorialStep[]
        {
            new TutorialStep
            {
                popup = new TutorialPopupData
                {
                    heading = "Welcome to Emberfall!",
                    description = $"{UserProfile.Instance.userName}, never heard that before. Welcome to emberfall! Emberfall is a td-ish game where you defend against the enemy, earn currency and spend that currency to upgrade your units and unlock new ones, rising through the difficulties until you reach the top! But enough talk, let's get going!"
                },
                onComplete = () => { PlayStep(1); }
            },
            new TutorialStep
            {
                target = "Btn_Play",
                popup = new TutorialPopupData
                {
                    heading = "Let's go!",
                    description = "Usually when you press play you need to select a difficulty, but since this is a tutorial I've taken care of it for you!\nGood luck!",
                },
                onComplete = () =>
                {
                    //Create tutorial scenario
                    GameSettingsService.Instance.SetDifficulty(DifficultyLevel.Tutorial);
                    SceneManager.LoadScene("Game");
                }
            }

        };
    }


    public void MainMenuReady()
    {
        if (!SaveService.Instance.Current.Flags.HasCompletedTutorial)
        {
            PlayStep(0);
        }

    }

    public void PlayStep(int step)
    {
        if (step >= steps.Length)
        {
            //TutorialCompleted();
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
