using System.Collections.Generic;
using UnityEngine;

public class HealthbarManager : MonoBehaviour
{
    public static HealthbarManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private FloatingHealthBar floatingHealthBarPrefab;
    [SerializeField] private Canvas healthBarCanvas;
    [SerializeField] private int initialPoolSize = 100;

    private Queue<FloatingHealthBar> availableHealthBars = new();
    private Dictionary<BaseUnitStats, FloatingHealthBar> activeHealthBars = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            FloatingHealthBar healthBar = Instantiate(floatingHealthBarPrefab, healthBarCanvas.transform);
            healthBar.Reset();
            availableHealthBars.Enqueue(healthBar);
        }
    }

    private void Update()
    {
        foreach (var kvp in activeHealthBars)
        {
            BaseUnitStats unit = kvp.Key;
            FloatingHealthBar healthBar = kvp.Value;

            if (unit == null || !unit.IsAlive)
            {
                ReturnHealthBar(unit);
            }
            else if (healthBar.IsActive)
            {
                healthBar.UpdatePosition(unit.Transform.position);
            }
        }
    }

    public FloatingHealthBar RequestHealthBar(BaseUnitStats unit, Vector3 offset)
    {
        // Check if unit already has an active healthbar
        if (activeHealthBars.TryGetValue(unit, out FloatingHealthBar existingBar))
        {
            existingBar.SetActive(true);
            return existingBar;
        }

        FloatingHealthBar healthBar;
        if (availableHealthBars.Count > 0)
        {
            healthBar = availableHealthBars.Dequeue();
        }
        else
        {
            healthBar = Instantiate(floatingHealthBarPrefab, healthBarCanvas.transform);
        }

        healthBar.Initialize(unit.Transform, offset);
        healthBar.UpdateHealthBar(unit.currentHealth, unit.MaxHealth);
        healthBar.SetActive(true);
        activeHealthBars[unit] = healthBar;

        return healthBar;
    }

    public void UpdateHealthBar(BaseUnitStats unit, float currentHealth, float maxHealth)
    {
        if (activeHealthBars.TryGetValue(unit, out FloatingHealthBar healthBar))
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    public void ReturnHealthBar(BaseUnitStats unit)
    {
        if (activeHealthBars.TryGetValue(unit, out FloatingHealthBar healthBar))
        {
            healthBar.SetActive(false);
            availableHealthBars.Enqueue(healthBar);
            activeHealthBars.Remove(unit);
        }
    }

    public void ShowHealthBar(BaseUnitStats unit)
    {
        if (activeHealthBars.TryGetValue(unit, out FloatingHealthBar healthBar))
        {
            healthBar.SetActive(true);
        }
    }

    public void HideHealthBar(BaseUnitStats unit)
    {
        if (activeHealthBars.TryGetValue(unit, out FloatingHealthBar healthBar))
        {
            healthBar.SetActive(false);
        }
    }
}
