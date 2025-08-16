using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleEx : Toggle
{
    public Sprite DeselectedSprite => deselectedSprite;
    [SerializeField] private Sprite deselectedSprite;
    public Sprite DeselectedToggleSprite => deselectedToggleSprite;
    [SerializeField] private Sprite deselectedToggleSprite;
    public Sprite SelectedToggleSprite => selectedToggleSprite;
    [SerializeField] private Sprite selectedToggleSprite;
    public Image ToggleBackgroundImage => toggleBackgroundImage;
    [SerializeField] private Image toggleBackgroundImage;


    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        DoStateTransition(instant);
    }

    // Toggle.OnValueChanged에서 부르긴 해야 함 (그래야 false여도 갱신 됨)
    public void DoStateTransition(bool instant)
    {
        // Debug.Log($"{gameObject} : {instant}");
        if (gameObject == null || !gameObject.activeInHierarchy)
        {
            return;
        }

        Color tintColor;
        Sprite transitionSprite;
        Sprite toggleBackgroundSprite = isOn ? selectedToggleSprite : deselectedToggleSprite; ;
        string triggerName;

        switch (currentSelectionState)
        {
            case SelectionState.Normal:
                tintColor = colors.normalColor;
                transitionSprite = isOn ? spriteState.selectedSprite : deselectedSprite;
                triggerName = animationTriggers.normalTrigger;
                break;
            case SelectionState.Highlighted:
                tintColor = colors.normalColor;
                transitionSprite = isOn ? spriteState.selectedSprite : deselectedSprite;
                triggerName = animationTriggers.highlightedTrigger;
                break;
            case SelectionState.Pressed:
                tintColor = colors.pressedColor;
                transitionSprite = isOn ? spriteState.selectedSprite : spriteState.pressedSprite;
                triggerName = animationTriggers.pressedTrigger;
                break;
            case SelectionState.Selected:
                tintColor = colors.selectedColor;
                transitionSprite = isOn ? spriteState.selectedSprite : deselectedSprite;
                triggerName = animationTriggers.selectedTrigger;
                break;
            case SelectionState.Disabled:
                tintColor = colors.disabledColor;
                transitionSprite = isOn ? spriteState.selectedSprite : spriteState.disabledSprite;
                triggerName = animationTriggers.disabledTrigger;
                break;
            default:
                tintColor = Color.black;
                transitionSprite = null;
                triggerName = string.Empty;
                break;
        }

        switch (transition)
        {
            case Transition.ColorTint:
                StartColorTween(tintColor * colors.colorMultiplier, instant);
                break;
            case Transition.SpriteSwap:
                DoSpriteSwap(transitionSprite, toggleBackgroundSprite);
                break;
            case Transition.Animation:
                TriggerAnimation(triggerName);
                break;
        }
    }

    void StartColorTween(Color targetColor, bool instant)
    {
        if (targetGraphic == null)
            return;

        targetGraphic.CrossFadeColor(targetColor, instant ? 0f : colors.fadeDuration, true, true);
    }

    void DoSpriteSwap(Sprite newSprite, Sprite toggleBackgroundSprite)
    {
        if (image == null)
            return;

        image.overrideSprite = newSprite;
        toggleBackgroundImage.overrideSprite = toggleBackgroundSprite;
    }

    void TriggerAnimation(string triggername)
    {
#if PACKAGE_ANIMATION
        if (transition != Transition.Animation || animator == null || !animator.isActiveAndEnabled || !animator.hasBoundPlayables || string.IsNullOrEmpty(triggername))
            return;

        animator.ResetTrigger(animationTriggers.normalTrigger);
        animator.ResetTrigger(animationTriggers.highlightedTrigger);
        animator.ResetTrigger(animationTriggers.pressedTrigger);
        animator.ResetTrigger(animationTriggers.selectedTrigger);
        animator.ResetTrigger(animationTriggers.disabledTrigger);

        animator.SetTrigger(triggername);
#endif
    }

    private void OnSetProperty()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            DoStateTransition(currentSelectionState, true);
        else
#endif
            DoStateTransition(currentSelectionState, false);
    }
}

internal static class SetPropertyUtility
{
    public static bool SetColor(ref Color currentValue, Color newValue)
    {
        if (currentValue.r == newValue.r && currentValue.g == newValue.g && currentValue.b == newValue.b && currentValue.a == newValue.a)
            return false;

        currentValue = newValue;
        return true;
    }

    public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
    {
        if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
            return false;

        currentValue = newValue;
        return true;
    }

    public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
    {
        if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
            return false;

        currentValue = newValue;
        return true;
    }
}