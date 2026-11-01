using System;
using System.Collections.Generic;
using UnityEngine;

public class chunkController : MonoBehaviour
{
    [Header("Spawbing Editor Items")]

    [SerializeField]
    private bool isSpawningEditorItems = true;
    [Tooltip("The Layers we will spawn the items from - they will take up cells")]
    [SerializeField]
    private LayerMask layers;


    [Header("Chunk Settings")]
    [SerializeField]
    private float moistureLevel = 1f; //1 is ideal

    private Dictionary<Vector3Int, GameObject> itemCells;
    // keyed by grid cell, because world positions are floats
    private Dictionary<Vector3Int, abstractPlant> occupiedCells;

    // the root GameObject that was spawned for each cell
    private Dictionary<Vector3Int, GameObject> cellObjects;

    //yet to be removed
    private List<Vector3Int> occupiedCellsToRemove;

    // the count of plants influences the drop rate of seeds
    private Dictionary<plantSeed, int> plantCounts = new Dictionary<plantSeed, int>();

    [Header("Chunk Settings")]
    [SerializeField]
    private GameObject ground;
    [SerializeField]
    private Grid grid;
    [SerializeField]
    private float evaporationRate = 0.1f;
    [SerializeField]
    private ParticleSystem rainParticleSystem;
    [SerializeField]
    private float rainChance = 0f;
    [SerializeField]
    private float rainModifier = 1f; //to be used by atua
    [SerializeField]
    [Range(0f, 1f)]
    private float removalSeedChance = 0.5f; //chance of getting the seed back when the player removes a plant

    [Header("Events")]
    [SerializeField]
    seedEvent seedPlanted; //raised after a plant is placed in this chunk
    [SerializeField]
    plantEvent plantRemoved; //raised just before a removed or dead plant is destroyed
    [SerializeField]
    seedEvent seedCollected; //the same event a picked-up seed uses, so the inventory adds removal seeds the same way

    [Header("Rain Settings")]
    //chance of rain on any given day
    // avg rain days per year in nz is 66 - 185 (https://figure.nz/chart/9dXdcHwVoaE17eVO)
    // so take the average (125.5) and divide it by the days in a year (125.5/365)
    // I took the data from https://niwa.co.nz/climate-and-weather/mean-monthly-rainfall-mm and averaged a range of 90.5 - 125.8 by month
    // 90.5/30 * 1.39 = 125.8, so that is the multiplier range applied to 90.5
    [SerializeField]
    const float rainPercentage = 0.34383561643f;
    [SerializeField]
    const float rainAmount = 3.01666666667f;
    const float maxRainMultiplier = 1.39f;
    const float minRainMultiplier = 1f;

    const float rainParticleCount = 30;
    float currentRainfall = 0;

    private bool playerInChunk = false;
    const float planeBound = 5f; //a unity plane always has a 10x10 bound, so half of that

    // in Awake because event listeners register in OnEnable, which runs before Start - the collections exist before any request can arrive
    void Awake()
    {
        occupiedCells = new Dictionary<Vector3Int, abstractPlant>();
        cellObjects = new Dictionary<Vector3Int, GameObject>();
        occupiedCellsToRemove = new List<Vector3Int>();
        itemCells = new Dictionary<Vector3Int, GameObject>();
    }

    void Start()
    {
        if (currentRainfall == 0)
        {
            rainParticleSystem.Stop();
        }

        if (isSpawningEditorItems)
        {
            spawnEditorItems();
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (currentRainfall > 0)
        {
            if (playerInChunk)
            {
                Debug.Log("player in chunk, playing rain");
                rainParticleSystem.Play();
            }
            else
            {
                rainParticleSystem.Stop();
            }

        }
        else if (rainParticleSystem.isPlaying)
        {
            rainParticleSystem.Stop();
        }

        foreach (var (cell, plant) in occupiedCells)
        {
            // plant == null catches plants that were destroyed - there's gotta be a better way
            if (plant == null || !plant.isAlive())
            {
                occupiedCellsToRemove.Add(cell);
            }
        }

        // Capacity is how much space the list has reserved, so check count for how many items are actually in it
        if (occupiedCellsToRemove.Count > 0)
        {
            foreach (Vector3Int cell in occupiedCellsToRemove)
            {

                removePlantInCell(cell);
            }


            occupiedCellsToRemove.Clear();
        }

    }


    void spawnEditorItems()
    {

        // we create a box the size of this chunk's ground, reaching a few units above it so all objects are caught.
        Vector3 halfSize = new Vector3(planeBound * ground.transform.lossyScale.x, 3f, planeBound * ground.transform.lossyScale.z);
        Vector3 centre = ground.transform.position + Vector3.up * 2f;

        Collider[] found = Physics.OverlapBox(centre, halfSize, ground.transform.rotation, layers, QueryTriggerInteraction.Collide);

        // an object can have more than one collider, so this remembers which objects have already been done
        HashSet<GameObject> done = new HashSet<GameObject>();

        foreach (Collider col in found)
        {
            abstractPlant plant = col.GetComponentInParent<abstractPlant>();

            GameObject obj;

            if (plant != null)
            {
                obj = plant.gameObject;
            }
            else
            {
                obj = col.gameObject;
            }

            if (done.Contains(obj))
            {
                continue;
            }

            done.Add(obj);

            // make sure it's in THIS chunk
            if (!isInChunk(obj.transform.position))
            {
                continue;
            }

            Vector3Int cell = getCellFromWorldPosition(obj.transform.position);

            // the chunk only tracks one thing per cell so the extra one is removed
            if (cellOccupied(cell))
            {
                Debug.Log($"{obj.name} is in a cell ({cell}) that's already taken, it's been removed");

                Destroy(obj);
                continue;
            }

            // snap it to the middle of its cell
            obj.transform.position = getGroundPositionForCell(cell);

            //if it is a plant
            if (plant != null)
            {
                if (plant.getSeed() == null)
                {
                    Debug.Log($"{plant.name} has no seed set skipping adding to cell {cell}");
                    Destroy(obj);
                    Destroy(plant);

                    continue;
                }

                //if it is ready to be place, place it
                addPlantToCell(cell, plant, obj);
                Debug.Log($"{plant.name} has been added to cell {cell}");

            }
            else
            {
                itemCells.Add(cell, obj);
                Debug.Log($"an item has been added to cell {cell}");

            }
        }
    }

    public void onDayChange(int amount)
    {
        rainChance = UnityEngine.Random.Range(0f, 1f);
        Debug.Log($"rainChance = {rainChance}");

        if (rainChance < rainPercentage)
        {
            makeRain(rainModifier);
        }
        else
        {
            currentRainfall = 0;
        }

    }

    public void makeRain(float modifierAmount)
    {
        currentRainfall = rainAmount * UnityEngine.Random.Range(minRainMultiplier, maxRainMultiplier) * modifierAmount;
        Debug.Log($"currentRainfall = {currentRainfall}");

        var emission = rainParticleSystem.emission;
        emission.rateOverTime = (float)rainParticleCount * currentRainfall;

        moistureLevel += currentRainfall;
    }

    public void playerInChunkCheck(Vector3 worldPlayerPosition)
    {

        Vector3 localPosition = ground.transform.InverseTransformPoint(worldPlayerPosition); //gets player pos relative to the ground

        playerInChunk = localPosition.x >= -planeBound && localPosition.x <= planeBound &&
                localPosition.z >= -planeBound && localPosition.z <= planeBound;

    }


    public void onSunshineHourPassed(int amount)
    {
        Debug.Log($"{amount} sunshine hour has passed in chunk");

        float newAmount = moistureLevel -= evaporationRate * amount;
        moistureLevel = Math.Clamp(newAmount, -1, 1);


        foreach (var (cell, plant) in occupiedCells)
        {
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
        foreach (var (cell, placedObject) in occupiedCells)
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

    // placing & removing plants/items

    // huses rhe Place Requested event through a placementRequestEventListener on this chunk
    // every chunk hears every request, so the first job is checking the spot is in this chunk
    public void onPlaceRequested(placementRequest request)
    {
        if (request.seed == null || request.seed.plantPrefab == null)
        {
            return;
        }
        if (!isInChunk(request.worldPosition))
        {
            return;
        }

        Vector3Int cell = getCellFromWorldPosition(request.worldPosition);
        if (cellOccupied(cell))
        {
            return;
        }

        // not parented to the chunk, to not mess with scale
        GameObject spawned = Instantiate(request.seed.plantPrefab, getGroundPositionForCell(cell), Quaternion.identity);
        abstractPlant plant = spawned.GetComponentInChildren<abstractPlant>();
       
        if (plant == null)
        {
            Destroy(spawned);
            return;
        }

        // makes sure the plant knows which seed it came from, even if the prefab's field was left empty
        plant.setSeed(request.seed);

        addPlantToCell(cell, plant, spawned);
        seedPlanted.Invoke(request.seed);
    }

    // hooked up to the removeRequested event through a plantEventListener on this chunk
    public void onRemoveRequested(abstractPlant plant)
    {
        // the plant is passed in, so find which cell of this chunk holds it - also it might belong to another chunk.
        // the cell is found first and removed after the loop-  changing a dictionary while looping over it throws an error
        Vector3Int? foundCell = null;
        foreach (var (cell, cellPlant) in occupiedCells)
        {
            if (cellPlant == plant)
            {
                foundCell = cell;
                break;
            }
        }

        if (foundCell.HasValue)
        {

            // read what's needed before the plant is removed
            plantSeed seed = plant.getSeed();

            removePlantInCell(foundCell.Value);

            if (UnityEngine.Random.value < removalSeedChance)
            {
                seedCollected.Invoke(seed);
            }
        }
    }

    void addPlantToCell(Vector3Int cell, abstractPlant plant, GameObject root)
    {
        occupiedCells.Add(cell, plant);
        cellObjects.Add(cell, root);
    }

    // removePlantInCell destroys the gameobject. otherise a dead plant would stay visible forever

    void removePlantInCell(Vector3Int cell)
    {
        if (!occupiedCells.TryGetValue(cell, out abstractPlant plant))
        {
            return;
        }
        occupiedCells.Remove(cell);

        // invoke before destroy in case something is listening
        if (plant != null)
        {
            plantRemoved.Invoke(plant);
        }

        if (cellObjects.TryGetValue(cell, out GameObject root))
        {
            cellObjects.Remove(cell);
            if (root != null)
            {
                Destroy(root);
            }
        }
    }


    public bool isInChunk(Vector3 worldPosition)
    {
        Vector3 localPosition = ground.transform.InverseTransformPoint(worldPosition);

        // < on the far edges instead of <=, so a point exactly on the border between two chunks only belongs to one of them
        return localPosition.x >= -planeBound && localPosition.x < planeBound &&
               localPosition.z >= -planeBound && localPosition.z < planeBound;
    }

    public abstractPlant getPlantInCell(Vector3Int cell)
    {
        return occupiedCells.TryGetValue(cell, out abstractPlant plant) ? plant : null;
    }

    public GameObject getItemInCell(Vector3Int cell)
    {
        return itemCells.TryGetValue(cell, out GameObject item) ? item : null;
    }

    // the grid's cell centre sits halfway up the cell's y this drops it down onto the ground plane
    public Vector3 getGroundPositionForCell(Vector3Int cell)
    {
        Vector3 position = getCenterCellWorldPosition(cell);
        position.y = ground.transform.position.y;
        return position;
    }

    // a random spot on this chunk's ground - used for seed spawn
    public Vector3 getRandomPosInChunk()
    {
        Vector3 localPoint = new Vector3(
            UnityEngine.Random.Range(-planeBound, planeBound),
            0f,
            UnityEngine.Random.Range(-planeBound, planeBound));
        return ground.transform.TransformPoint(localPoint);
    }

    // how many living plants of each seed type are in this chunk - used to decide which seeds drop here.
    public Dictionary<plantSeed, int> getPlantCounts()
    {
        plantCounts.Clear();
        foreach (abstractPlant plant in occupiedCells.Values)
        {
            if (plant == null || !plant.isAlive() || plant.getSeed() == null)
            {
                continue;
            }

            plantSeed seed = plant.getSeed();
            plantCounts.TryGetValue(seed, out int count); //count is 0 if this seed isn't in there yet
            plantCounts[seed] = count + 1;
        }
        return plantCounts;
    }

    public void onPlantEaten(plantEat evt)
    {
        if (evt.plant != null && getCellOfPlant(evt.plant, out _)) //this is cool, you learn something new everyday
        {
            evt.plant.eat(evt.amount);
        }
    }

    // hooked up to the Target Queried event. the player asks what's at a point, and only the chunk that owns the point answers
    public void onTargetQueried(chunkQuery query)
    {
        if (!isInChunk(query.point))
        {
            return;
        }

        Vector3Int cell = getCellFromWorldPosition(query.point);
        GameObject item = getItemInCell(cell);

        query.answered = true;
        query.cellPosition = getGroundPositionForCell(cell);
        query.occupied = cellOccupied(cell);
        query.plant = getPlantInCell(cell);
        query.npc = item != null ? item.GetComponent<npc>() : null;

        if (query.npc != null)
        {
            query.name = item.name;
        }
        else if (query.plant != null && query.plant.getSeed() != null)
        {
            query.name = query.plant.getSeed().displayName;
        }
        else
        {
            query.name = "unknown plant";
        }
    }

    // hooked up to the PlantQuery event
    public void onPlantQueryRequested(plantQuery query)
    {
        foreach (abstractPlant plant in occupiedCells.Values)
        {
            if (!isEdible(plant, query.data))
            {
                continue;
            }

            float distance = Vector3.Distance(plant.transform.position, query.position);
            if (distance <= query.radius && distance < query.nearestDistance)
            {
                query.nearest = plant;
                query.nearestPosition = plant.transform.position;
                query.nearestDistance = distance;
            }
        }
    }

    // this used to be in findPlant.cs
    bool isEdible(abstractPlant plant, animalData data)
    {
        if (plant == null || !plant.isAlive())
        {
            return false;
        }
        if (plant.getGrowthPercent() < data.minPlantGrowthPercent)
        {
            return false;
        }
        // an empty list means this animal eats anything
        return data.edibleSeeds.Count == 0 || data.edibleSeeds.Contains(plant.getSeed());
    }

    bool getCellOfPlant(abstractPlant plant, out Vector3Int foundCell)
    {
        foreach (var (cell, cellPlant) in occupiedCells)
        {

            if (cellPlant == plant)
            {
                foundCell = cell;
                return true;
            }

        }
        foundCell = default;
        return false;

    }

    // public so the player and the seed dropper can read the grid
    public Vector3Int getCellFromWorldPosition(Vector3 position)
    {
        return grid.WorldToCell(position);
    }

    public Vector3 getCenterCellWorldPosition(Vector3Int cell)
    {
        return grid.GetCellCenterWorld(cell);
    }

    public bool cellOccupied(Vector3Int key)
    {
        return occupiedCells.ContainsKey(key) || (itemCells.TryGetValue(key, out GameObject obj) && obj != null);
    }

}