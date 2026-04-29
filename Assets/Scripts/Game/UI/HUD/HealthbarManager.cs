using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HealthbarManager : MonoBehaviour
{
    public static HealthbarManager Instance { get; private set; }

    [SerializeField] private FloatingHealthBar prefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera cam;
    [SerializeField] private int poolSize = 100;

    private readonly Queue<FloatingHealthBar> pool = new();
    private readonly Dictionary<BaseUnitStats, FloatingHealthBar> active = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            FloatingHealthBar healthBar = Instantiate(prefab, canvas.transform);
            healthBar.gameObject.SetActive(false);
            pool.Enqueue(healthBar);
        }
    }

    private void LateUpdate()
    {
        foreach (var kvp in active)
        {
            BaseUnitStats unit = kvp.Key;
            FloatingHealthBar healthBar = kvp.Value;

            Vector3 worldPos = unit.GetHeadPosition();

            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                canvas.worldCamera,
                out Vector2 localPos
            );

            healthBar.SetPosition(localPos);
        }
    }

    public FloatingHealthBar RequestHealthBar(BaseUnitStats unit)
    {
        FloatingHealthBar healthBar;

        if (pool.Count > 0)
        {
            healthBar = pool.Dequeue();
        } 
        else
        {
            for(int i = 0; i < 10; i++) // Expand pool if empty
            {
                FloatingHealthBar newBar = Instantiate(prefab, canvas.transform);
                newBar.gameObject.SetActive(false);
                pool.Enqueue(newBar);
            }
            healthBar = pool.Dequeue();
        }

        active[unit] = healthBar;

        healthBar.gameObject.SetActive(true);

        healthBar.Initialize(unit.healthBarOffset, unit.healthBarScale);

        return healthBar;
    }

    public void ReturnHealthBarToPool(BaseUnitStats unit)
    {
        if(active.TryGetValue(unit, out FloatingHealthBar healthBar))
        {
            healthBar.gameObject.SetActive(false);
            pool.Enqueue(healthBar);
            active.Remove(unit);
        }
    }
}