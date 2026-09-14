using UnityEngine;
using System.Collections.Generic;

// Drives the trainee through an ordered sequence of steps for a module (Fire or Gas).
// Each step is a MultipleChoiceStep component living under the placed scenario prefab.
// This script doesn't know the specific content of each step (cause/PPE/extinguisher/exit) —
// it just advances through whatever steps you've configured in the Inspector, in order.
//
// Owner: Riddhi (AR / Training)
// Flow this supports (as described): fire visuals appear -> "what caused the fire?" (icons,
// e.g. microwave) -> PPE selection -> fire extinguisher selection -> exit selection.

public class TrainingStateMachine : MonoBehaviour
{
    [Tooltip("Steps in the order the trainee should complete them. Assign in the Inspector once your scenario prefab is set up.")]
    public List<MultipleChoiceStep> steps = new List<MultipleChoiceStep>();

    [Tooltip("Fired when all steps are complete — hook this to your assessment/scoring screen.")]
    public UnityEngine.Events.UnityEvent onModuleComplete;

    private int currentStepIndex = -1;
    private int correctCount = 0;

    // Called by PlaneTapToPlace once the scenario prefab has been placed in the world.
    public void OnScenarioPlaced(GameObject placedScenario)
    {
        // If steps weren't assigned in the Inspector, try to find them under the placed prefab
        if (steps.Count == 0)
        {
            steps.AddRange(placedScenario.GetComponentsInChildren<MultipleChoiceStep>(includeInactive: true));
        }

        foreach (var step in steps)
        {
            step.gameObject.SetActive(false);
            step.OnAnswered += HandleStepAnswered;
        }

        AdvanceToNextStep();
    }

    void AdvanceToNextStep()
    {
        currentStepIndex++;

        if (currentStepIndex >= steps.Count)
        {
            onModuleComplete?.Invoke();
            Debug.Log($"Module complete. Correct: {correctCount}/{steps.Count}");
            return;
        }

        steps[currentStepIndex].gameObject.SetActive(true);
        steps[currentStepIndex].BeginStep();
    }

    void HandleStepAnswered(bool wasCorrect)
    {
        if (wasCorrect) correctCount++;

        steps[currentStepIndex].gameObject.SetActive(false);
        AdvanceToNextStep();
    }

    public int GetScore() => correctCount;
    public int GetTotalSteps() => steps.Count;
}