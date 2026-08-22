using System;
using System.Collections.Generic;
using UnityEngine;

public class chunkController : MonoBehaviour
{
    [Header("For Testing")]

    [SerializeField]
    bool spawnPlaceHolder = true;
    [SerializeField]
    GameObject placeholder;



    [Header("Chunk State")]
    [SerializeField]
    public float moistureLevel = 1.0f; //1 is ideal
    private Dictionary<Vector3, abstractPlant> occupiedCells;
    private List<Vector3> occupiedCellsToRemove;

    [Header("Chunk Settings")]
    [SerializeField]
    public GameObject ground;
    [SerializeField]
    public Grid grid;
    [SerializeField]
    float evaporationRate = 0.1f;
    [SerializeField]
    ParticleSystem rainParticleSystem;
    [SerializeField]
    float rainChance = 0.000f;
    [SerializeField]
    float rainModifier = 1f; //to be used by atua

    [Header("Rain Settings")]
    [SerializeField]
    const float rainPercentage = 0.43424657534f;
    //for determining if raining
    // avg rain days per year in nz is 66 - 185 (https://figure.nz/chart/9dXdcHwVoaE17eVO)
    // so lets take the average - 158.5, and use that to determine whether or not it will be a rainy day in this chunk (158.5/365 = 0.43424657534)
    [SerializeField]
    const float rainAmount = 3.01666666667f;
    const float maxRainMultiplier = 1.39f;
    const float minRainMultiplier = 1.0f;
    // I took the data from https://niwa.co.nz/climate-and-weather/mean-monthly-rainfall-mm and averaged a range of 90.5 - 125.8 by month
    // 90.5/30 * 1.39 = 125.8, so we will use this as the range for a random number to multiply 90.5 by.
    const float rainParticleCount = 30;
    float currentRainfall = 0;

    private bool playerInChunk = false;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        occupiedCells = new Dictionary<Vector3, abstractPlant>();
        occupiedCellsToRemove = new List<Vector3>();

        if (currentRainfall == 0)
        {
            rainParticleSystem.Stop();
        }

        if (spawnPlaceHolder)
        {
            GameObject temp = Instantiate(placeholder);
            if (temp.GetComponentInChildren<abstractPlant>() == null)
            {
                Debug.LogWarning("No abstractPlant script found on the object!");
            }
            occupiedCells.Add(temp.transform.position, temp.GetComponentInChildren<abstractPlant>());
        }
    }

    // Update is called once per frame 
    void Update()
    {

        if (currentRainfall > 0)
        {
            if (playerInChunk)
            {
                rainParticleSystem.Play();
            }
            else
            {
                rainParticleSystem.Stop();
            }

        }

        foreach (var (location, plant) in occupiedCells)
        {
            if (!plant.isAlive())
            {
                occupiedCellsToRemove.Add(location);
            }
        }

        if (occupiedCellsToRemove.Capacity > 0)
        {
            foreach (Vector3 location in occupiedCellsToRemove)
            {
                occupiedCells.Remove(location);
            }

            occupiedCellsToRemove.Clear();
            //garbage collection should take care of disposing the gamobject

        }

    }

    public void onDayChange(int amount)
    {
        rainChance = UnityEngine.Random.Range(0.00000000000f, 1f);
        Debug.Log($"rainChance = {rainChance}");

        if (rainChance < rainPercentage)
        {
            currentRainfall = rainAmount * UnityEngine.Random.Range(minRainMultiplier, maxRainMultiplier) * rainModifier;
            Debug.Log($"currentRainfall = {currentRainfall}");

            var emission = rainParticleSystem.emission;
            emission.rateOverTime = (float)rainParticleCount * currentRainfall;

            moistureLevel += currentRainfall;
        }
        else
        {
            currentRainfall = 0;
        }

    }

    public void playerInChunkCheck(Vector3 worldPlayerPosition)
    {

        Vector3 localPosition = ground.transform.InverseTransformPoint(worldPlayerPosition); //gets player pos relative to the ground

        float planeBound = 5f; //a unity plane always has a 10x10 bound, so half of that

        playerInChunk = localPosition.x >= -planeBound && localPosition.x <= planeBound &&
                localPosition.z >= -planeBound && localPosition.z <= planeBound;

    }


    public void onSunshineHourPassed(int amount)
    {
        Debug.Log($"{amount} sunshine hour has passed in chunk");

        float newAmount = moistureLevel -= evaporationRate * amount;
        moistureLevel = Math.Clamp(newAmount, -1, 1);
    

        foreach (var (location, plant) in occupiedCells)
        {
            Debug.Log($"found plant in occupiedCells");

            plant.increaseAliveHours(amount);

            if (plant.isIdealConditions(moistureLevel))
            {
                plant.grow(plant.getIdealConditionsBoost() * amount);
            }
            else
            {
                plant.grow(amount);
            }


        }
    }

    public void onHourChange(int amount)
    {
        foreach (var (location, placedObject) in occupiedCells)
        {
            if (placedObject is abstractPlant plant)
            {
                if (moistureLevel > 0)
                {
                    moistureLevel -= plant.usedWater(amount);
                }
                else
                {
                    plant.damage(amount);
                }
            }
        }
    }

    void placeObject(IPlaceable obj)
    {

    }

    void setRainModifier(float amount)
    {
        rainModifier = amount;
    }

    Vector3Int getCellFromWorldPosition(Vector3 position)
    {
        return grid.WorldToCell(position);
    }

    Vector3 getCenterCellWorldPosition(Vector3Int cell)
    {
        return grid.GetCellCenterWorld(cell);
    }

    bool cellOccupied(Vector3Int key)
    {
        return occupiedCells.ContainsKey(key);
    }

}
