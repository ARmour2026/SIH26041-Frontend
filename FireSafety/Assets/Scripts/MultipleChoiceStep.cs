using UnityEngine;
using System;
using System.Collections.Generic;

// One "question" in the training sequence — e.g. "What caused the fire?" with icon choices
// like microwave, wiring, gas cylinder. Reused for cause-of-fire, PPE selection, extinguisher
// selection, and exit selection — only the icons/answers change per step (set in Inspector).
//
// Owner: Aashi (UI) with Riddhi (AR/Training) for wiring into the state machine.

[Serializable]
public class IconChoice
{
    public string label;           // e.g. "Microwave", "Wiring short circuit", "Gas cylinder"
    public Sprite icon;
    public bool isCorrectAnswer;
    [TextArea] public string voiceLineKey; // key into your AudioNarrator clip dictionary
}

public class MultipleChoiceStep : MonoBehaviour
{
    [Tooltip("Optional 3D visual to show for this step, e.g. the fire VFX or the microwave model.")]
    public GameObject stepVisual;

    [Tooltip("If true (default), stepVisual is hidden automatically once this step is answered. " +
             "Uncheck this for the fire on the Cause-of-Fire step, since the fire should keep " +
             "burning across PPE/Extinguisher steps and only go out when the trainee picks the " +
             "correct extinguisher — wire that up with 'On Correct Answer' below instead.")]
    public bool hideVisualOnFinish = true;

    [Tooltip("Fires only when the trainee picks the CORRECT choice on this step. Use this on the " +
             "Extinguisher step to call SetActive(false) (or Stop()) on the fire VFX object.")]
    public UnityEngine.Events.UnityEvent onCorrectAnswerSelected;

    [Tooltip("The icon options the trainee can tap. Mark exactly one as the correct answer.")]
    public List<IconChoice> choices = new List<IconChoice>();

    [Tooltip("Prefab for a single icon button — must have IconChoiceButton component.")]
    public IconChoiceButton iconButtonPrefab;

    [Tooltip("Where the generated icon buttons should appear (a UI panel/layout group).")]
    public Transform choiceButtonParent;

    [Tooltip("Voice line key played when this step begins (e.g. 'ask_fire_cause').")]
    public string introVoiceLineKey;

    // The state machine listens for this to know whether to move on
    public event Action<bool> OnAnswered;

    private List<IconChoiceButton> spawnedButtons = new List<IconChoiceButton>();
    private bool answered = false;

    public void BeginStep()
    {
        answered = false;

        if (stepVisual != null) stepVisual.SetActive(true);

        AudioNarrator.Instance?.PlayLine(introVoiceLineKey);

        SpawnChoiceButtons();
    }

    void SpawnChoiceButtons()
    {
        ClearButtons();

        foreach (var choice in choices)
        {
            var button = Instantiate(iconButtonPrefab, choiceButtonParent);
            button.Setup(choice, OnChoiceSelected);
            spawnedButtons.Add(button);
        }
    }

    void OnChoiceSelected(IconChoice choice)
    {
        if (answered) return; // ignore extra taps after answering
        answered = true;

        AudioNarrator.Instance?.PlayLine(choice.isCorrectAnswer ? "feedback_correct" : "feedback_incorrect");

        if (choice.isCorrectAnswer)
        {
            onCorrectAnswerSelected?.Invoke();
        }

        foreach (var button in spawnedButtons)
        {
            button.ShowResultState(button.Choice == choice, choice.isCorrectAnswer);
        }

        // Small delay so the trainee sees the right/wrong feedback before moving on
        Invoke(nameof(FinishStep), 1.2f);

        // Cache the result to send once the delay completes
        pendingResult = choice.isCorrectAnswer;
    }

    private bool pendingResult;

    void FinishStep()
    {
        if (stepVisual != null && hideVisualOnFinish) stepVisual.SetActive(false);
        ClearButtons();
        OnAnswered?.Invoke(pendingResult);
    }

    void ClearButtons()
    {
        foreach (var b in spawnedButtons) Destroy(b.gameObject);
        spawnedButtons.Clear();
    }
}