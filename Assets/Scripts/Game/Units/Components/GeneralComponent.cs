using UnityEngine;

public class GeneralComponent : MonoBehaviour
{
    private GeneralDefinition generalDefinition;
    private MovementComponent movement;
    private int generalWave;
    private SpeechBubbleComponent speechBubble;

    private void Awake()
    {
        movement = GetComponent<MovementComponent>();
    }

    public void Initialize(GeneralDefinition generalDefinition, Transform bossPosition, int currentWaveIndex)
    {
        this.generalDefinition = generalDefinition;
        this.generalWave = generalDefinition.spawnLimit + currentWaveIndex;

        if (WaveController.waveGenerator != null)
        {
            WaveController.waveGenerator.OnWaveNumberChanged += SendGeneral;
        }

        speechBubble = GetComponent<SpeechBubbleComponent>();

        if (speechBubble != null)
        {
            int phraseCount = generalDefinition.tauntPhrases.Count;

            // Total time the general is expected to be on screen.
            int generalSecondsOnScreen = (generalDefinition.spawnLimit * 10) - 2;

            // The first and last phrases are handled separately.
            int middlePhraseCount = phraseCount - 2;

            // Time available between the initial phrase and the final phrase.
            float timeBetweenTaunts = (float)generalSecondsOnScreen / (phraseCount - 1);

            StartCoroutine(CoroutineHelpers.DoAfterDelay(2, () =>
            {
                speechBubble.Speak(generalDefinition.tauntPhrases[0]);
            }));

            StartCoroutine(CoroutineHelpers.DoAfterDelay(5, () =>
            {
                speechBubble.StopSpeaking();
            }));

            // Schedule all middle phrases.
            for (int i = 1; i <= middlePhraseCount; i++)
            {
                int tauntIndex = i;
                float delay = 2 + (timeBetweenTaunts * i);

                StartCoroutine(CoroutineHelpers.DoAfterDelay(delay, () =>
                {
                    speechBubble.Speak(generalDefinition.tauntPhrases[tauntIndex]);
                }));

                StartCoroutine(CoroutineHelpers.DoAfterDelay(delay + 5, () =>
                {
                    speechBubble.StopSpeaking();
                }));
            }
        }

        MoveToSpawnPosition(bossPosition.position);
    }

    private void OnDestroy()
    {
        if (WaveController.waveGenerator != null)
        {
            WaveController.waveGenerator.OnWaveNumberChanged -= SendGeneral;
        }
    }


    public void MoveToSpawnPosition(Vector2 position)
    {
        movement.SetTemporaryDestination(position);
    }

    public void SendGeneral(int waveNumber)
    {
        if (waveNumber == generalWave)
        {
            speechBubble.Speak(generalDefinition.tauntPhrases[generalDefinition.tauntPhrases.Count - 1]);

            StartCoroutine(CoroutineHelpers.DoAfterDelay(5, () =>
            {
                speechBubble.StopSpeaking();
            }));

            movement.ClearTemporaryDestination();
            movement.SetMovementEnabled(true);
        }
    }

}
