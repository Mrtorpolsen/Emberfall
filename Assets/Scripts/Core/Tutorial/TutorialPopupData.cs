public class TutorialPopupData
{
    public string imgAddress = null;
    public string heading = null;
    public string description = null;
    public bool hasButton = false;
    public TutorialPopupLocation position = TutorialPopupLocation.Auto;
}

public enum TutorialPopupLocation
{
    Auto,
    Top,
    Bottom
}