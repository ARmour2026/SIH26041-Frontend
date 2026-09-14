using UnityEngine;
using UnityEngine.UI;
using System;

// A single tappable icon button (e.g. the microwave icon, the PPE helmet icon).
// Attach to a prefab with an Image + Button component.
//
// Owner: Aashi (UI)

[RequireComponent(typeof(Button))]
public class IconChoiceButton : MonoBehaviour
{
    public Image iconImage;
    public Image backgroundImage; // used to flash green/red feedback
    public Color correctColor = new Color(0.6f, 1f, 0.6f);
    public Color incorrectColor = new Color(1f, 0.6f, 0.6f);
    public Color defaultColor = Color.white;

    public IconChoice Choice { get; private set; }

    private Action<IconChoice> onSelected;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Setup(IconChoice choice, Action<IconChoice> callback)
    {
        Choice = choice;
        onSelected = callback;

        if (iconImage != null) iconImage.sprite = choice.icon;
        if (backgroundImage != null) backgroundImage.color = defaultColor;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onSelected?.Invoke(Choice));
    }

    // Called after any choice is made — highlights this button green if it was the
    // correct answer, red if it was the wrong one the trainee picked.
    public void ShowResultState(bool wasThisButtonPicked, bool thisButtonWasCorrect)
    {
        button.interactable = false;

        if (backgroundImage == null) return;

        if (thisButtonWasCorrect)
            backgroundImage.color = correctColor;
        else if (wasThisButtonPicked)
            backgroundImage.color = incorrectColor;
    }
}