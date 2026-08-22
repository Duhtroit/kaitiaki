using System;
using System.IO.Compression;
using UnityEngine;

public abstract class abstractPlant : MonoBehaviour, IPlaceable
{
    [Header("Plant State")]
    [SerializeField]
    int aliveHours = 0;
    [SerializeField]
    float growthPercent = 1f;
    [SerializeField]
    float health = 100f;

    [Header("Plant Settings")]
    [SerializeField]
    int idealConditionsBoost = 2;

    // Bounce animation state
    [SerializeField]
    float bounce = 5.5f; //how much it overshoots
    [SerializeField]
    float bounceSpeed = 6f; // how fast it settles
    Vector3 scaleVelocity = Vector3.zero;


    public abstract float waterUseRate { get; } //never seen this before right now, but this forces an implementation to create this
    public abstract float growthRate { get; }
    public abstract float damageRate { get; }
    public abstract float idealMoisture { get; }
    public abstract float idealMoistureRange { get; }

    public ParticleSystem plantGrowthParticles;

    void Start()
    {
        plantGrowthParticles = GetComponentInChildren<ParticleSystem>();
        transform.localScale = Vector3.one * growthPercent;
    }

    void Update()
    {
        float target = growthPercent/100;

        float overshoot = target * bounce;

        if (transform.localScale.x < target)
        {
            transform.localScale = Vector3.SmoothDamp(transform.localScale,
                new Vector3(overshoot, overshoot, overshoot),
                ref scaleVelocity,
                1f / bounceSpeed
            );
        }
        else
        {
            transform.localScale = Vector3.SmoothDamp(transform.localScale,
                new Vector3(target, target, target),
                ref scaleVelocity,
                1f / bounceSpeed
            );
        }
    }


    public void grow(float amount)
    {
        Debug.Log($"Plant is growing by {amount}");
        if (growthPercent < 100)
        {
            growthPercent = Mathf.Clamp(growthPercent + (growthRate * amount), 0, 100);

            plantGrowthParticles.Play();
        }

    }

    public void damage(float amount)
    {
        health -= (damageRate * amount);

    }

    public void increaseAliveHours(int amount)
    {
        Debug.Log($"Plant is alive for {amount} more hours");

        aliveHours += amount;
    }

    public int getIdealConditionsBoost()
    {
        return idealConditionsBoost;
    }

    public bool isIdealConditions(float moistureLevel)
    {
        return Mathf.Abs(moistureLevel - idealMoisture) <= (idealMoistureRange / 2);
    }

    public bool isAlive()
    {
        return health > 0 ? true : false;
    }

    public float usedWater(int timeAmount)
    {
        Debug.Log($"Plant is using water");

        return waterUseRate * timeAmount;
    }

}
