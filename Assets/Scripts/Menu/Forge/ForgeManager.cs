using UnityEngine;
using UnityEngine.UIElements;

public class ForgeManager : MonoBehaviour
{
    private VisualElement root;
    private VisualElement forgePanel;
    private VisualElement talentTreePanel;

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        forgePanel = root.Q<VisualElement>("ForgePanel");
        talentTreePanel = root.Q<VisualElement>("TalentTreePanel");

        talentTreePanel.style.display = DisplayStyle.None;
    }

    public void OpenTalentTree()
    {
        forgePanel.style.display = DisplayStyle.None;
        talentTreePanel.style.display = DisplayStyle.Flex;

        //Setup for talentTree
    }

    public void BackToForge()
    {
        forgePanel.style.display = DisplayStyle.Flex;
        talentTreePanel.style.display = DisplayStyle.None;

        //Cleanup?
    }
}
