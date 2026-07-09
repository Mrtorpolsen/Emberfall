using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class ForgeView : IUIScreenView
{
    public VisualElement ForgePanel { get; private set; }
    public VisualElement TalentTreePanel { get; private set; }

    private TalentTreeView talentTreeView;

    public VisualTreeAsset unitContainer;
    private VisualElement unitsContainer;

    private const string UNITS_CONTAINER = "UnitsContainer";
    private const string FORGE_UNITCONTAINER_ADDRESSABLE = "UI/UnitContainer";

    private readonly List<(VisualElement element, EventCallback<ClickEvent> handler)> clickHandlers = new();

    public async Task InitializeAsync(VisualElement root)
    {
        ForgePanel = UtilityUIBinding.QRequired<VisualElement>(root, "ForgePanel");
        TalentTreePanel = UtilityUIBinding.QRequired<VisualElement>(root, "TalentTreePanel");

        talentTreeView = new TalentTreeView();
        talentTreeView.Initialize(TalentTreePanel);

        unitsContainer = UtilityUIBinding.QRequired<VisualElement>(root, UNITS_CONTAINER);
        unitContainer = await Addressables.LoadAssetAsync<VisualTreeAsset>(FORGE_UNITCONTAINER_ADDRESSABLE).Task;

        TalentTreePanel.style.display = DisplayStyle.None;
        ForgePanel.style.display = DisplayStyle.Flex;
    }

    public void RenderUnitContainers(List<UnitContainerDefinition> unitContainers)
    {
        ClearUnitsContainer();

        foreach (var unit in unitContainers)
        {
            VisualElement visualUnit = UtilityUIBinding.InstantiateRoot(unitContainer);

            var imgUnit = UtilityUIBinding.QRequired<VisualElement>(visualUnit, "Img_Unit");
            var btnUpg = UtilityUIBinding.QRequired<Button>(visualUnit, "Btn_Upg");

            UtilityLoadAddressable.LoadAddressableIcon(unit.img, imgUnit);
            UtilityUIBinding.BindVEClick(btnUpg, unit.onClick, clickHandlers);

            unitsContainer.Add(visualUnit);
        }
    }

    public void Cleanup()
    {
        talentTreeView.CleanupClicks();
    }

    public void ClearUnitsContainer()
    {
        CleanupClicks();
        unitsContainer.Clear();
    }

    public void CleanupClicks()
    {
        UtilityUIBinding.CleanupVEClicks(clickHandlers);
    }

    public TalentTreeView GetTalentTreeView() => talentTreeView;
    public VisualElement GetForgePanel() => ForgePanel;
    public VisualElement GetTalentTreePanel() => TalentTreePanel;
}
