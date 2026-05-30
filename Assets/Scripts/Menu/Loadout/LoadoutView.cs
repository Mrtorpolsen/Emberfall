using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class LoadoutView : IUIScreenView
{
    private VisualElement utilityRowContainer;
    private VisualElement towerRowContainer;
    private VisualElement unitRowContainer;

    private Label currentLoadoutHeading;

    public Task InitializeAsync(VisualElement root)
    {
        utilityRowContainer = UtilityUIBinding.QRequired<VisualElement>(root, "UtilityRowContainer");
        towerRowContainer = UtilityUIBinding.QRequired<VisualElement>(root, "TowerRowContainer");
        unitRowContainer = UtilityUIBinding.QRequired<VisualElement>(root, "UnitRowContainer");

        currentLoadoutHeading = UtilityUIBinding.QRequired<Label>(root, "Label_CurrentLoadout");
        
        return Task.CompletedTask;
    }
}