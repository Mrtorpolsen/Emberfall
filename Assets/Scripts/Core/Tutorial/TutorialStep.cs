using System;

public class TutorialStep
{
    public string target = null;
    public TutorialPopupData popup;
    public Action onComplete;
    public bool overrideOnClick = false;
    public TutorialInteraction interaction = TutorialInteraction.Click;
}

public enum TutorialInteraction
{
    Click,
    LongClick
}