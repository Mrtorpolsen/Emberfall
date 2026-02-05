using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;

[RequireComponent(typeof(Button))]
public class SpawnButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text unitText; 
    [SerializeField] private SpawnSide spawnSide;

    private AsyncOperationHandle<Sprite>? iconHandle;

    private GameObject unitPrefab;

    private float cost;
    private SpawnType spawnType;

    public SpawnSide SpawnSide => spawnSide;

    public void Setup(SpawnDefinition def)
    {
        unitText.text = def.DisplayName;
        costText.text = def.Cost.ToString();

        cost = def.Cost;
        spawnType = def.Type;

        unitPrefab = def.UnitPrefab;

        LoadIcon(def.Icon);
    }

    private void LoadIcon(AssetReference iconReference)
    {
        ReleaseIcon();

        if (iconReference == null || !iconReference.RuntimeKeyIsValid())
        {
            iconImage.sprite = null;
            return;
        }

        var handle = iconReference.LoadAssetAsync<Sprite>();
        iconHandle = handle;

        handle.Completed += op =>
        {
            // Button may have been destroyed or reused
            if (!this || !iconImage)
                return;

            if (op.Status == AsyncOperationStatus.Succeeded)
                iconImage.sprite = op.Result;
        };
    }

    private void ReleaseIcon()
    {
        if (iconHandle.HasValue)
        {
            Addressables.Release(iconHandle.Value);
            iconHandle = null;
        }
    }

    public void OnClick()
    {
        if (spawnType == SpawnType.Tower)
        {
            BuildingPlot plot = UIManager.Instance.GetActivePlot();

            if (plot == null || plot.IsOccupied)
            {
                return;
            }

            bool success = SpawnManager.Instance.SpawnSouthUnit(
                unitPrefab,
                unitPrefab.name.ToLowerInvariant(), 
                spawnType, 
                spawnSide
            );

            if (success)
            {
                plot.MarkOccupied();
            }
        }
        else
        {
            SpawnManager.Instance.SpawnSouthUnit(unitPrefab, unitPrefab.name.ToLowerInvariant(), spawnType, spawnSide);
        }
    }

    public void SetInteractable(float playerCurrency,  bool isPaused)
    {
        button.interactable = (!isPaused && playerCurrency >= cost);
    }

    private void OnDestroy()
    {
        ReleaseIcon();
    }
}
