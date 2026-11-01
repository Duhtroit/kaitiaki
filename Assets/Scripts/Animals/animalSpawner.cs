using System.Collections.Generic;
using UnityEngine;


public class animalSpawner : MonoBehaviour
{

    // every in-game hour there's a chance an animal shows up somewhere around the player.
    // a species can only be picked if there's something it eats near the player, which is what makes plants "attract" animals

    [Header("Species")]
    [SerializeField]
    List<animalData> species = new List<animalData>();

    [Header("Spawn Settings")]
    [SerializeField]
    [Range(0f, 1f)]
    float spawnChancePerHour = 0.3f;
    [SerializeField]
    int maxAnimals = 8;
    [SerializeField]
    float minSpawnDistance = 15f; //far enough that they arrive from off screen
    [SerializeField]
    float maxSpawnDistance = 25f;
    [SerializeField]
    float attractionRadius = 20f; //a species only comes if it has food within this distance of the player
    [SerializeField]
    LayerMask groundMask; //just the chunk ground layer, for finding spawn points
    [SerializeField]
    int spotAttempts = 5;
    [SerializeField]
    float groundRayHeight = 50f;

    [Header("Events")]
    [SerializeField]
    plantQueryEvent plantQueryRequested;

    List<animal> activeAnimals = new List<animal>();
    List<animalData> spawnableSpecies = new List<animalData>(); //reused every pick
    Vector3 playerPosition; //kept up to date by the On Player Position Change event

    // hooked up to the same position event the camera uses
    public void onPlayerPositionChange(Vector3 position)
    {
        playerPosition = position;
    }

    // hook this up to the same hour event the chunks use
    public void onHourChange(int amount)
    {
        // animals destroy themselves when they leave, which leaves "null" entries in the list. clear those out first
        activeAnimals.RemoveAll(existing => existing == null);

        // one roll per hour that passed, so skipping 3 hours at once still gives 3 chances
        for (int i = 0; i < amount; i++)
        {
            if (activeAnimals.Count >= maxAnimals)
            {
                return;
            }
            if (Random.value < spawnChancePerHour)
            {
                trySpawnAnimal();
            }
        }
    }

    public bool trySpawnAnimal()
    {
        animalData chosen = pickSpecies();
        if (chosen == null)
        {
            // nothing nearby to eat
            Debug.Log("the chosen animal couldn't spawn: no food");
            return false;
        }
        if (!findSpawnPoint(out Vector3 spawnPoint))
        {
            Debug.Log("the chosen animal couldn't spawn: no spawn point");
            return false;
        }

        // face the player on arrival, so they come in towards the garden rather than away from it
        Vector3 towardsPlayer = playerPosition - spawnPoint;
        towardsPlayer.y = 0f;
        animal spawned = Instantiate(chosen.prefab, spawnPoint, Quaternion.LookRotation(towardsPlayer));
        spawned.setup(chosen);
        activeAnimals.Add(spawned);

        Debug.Log($"A {chosen.displayName} was spawned");
        return true;
    }

    animalData pickSpecies()
    {
        // first, only keep species that have something to eat near the player
        spawnableSpecies.Clear();

        float totalWeight = 0f;
        foreach (animalData data in species)
        {
            if (data == null || data.prefab == null || data.spawnWeight <= 0f)
            {
                continue;
            }

            // ask the chunks if this species has food near the player
            plantQuery search = new plantQuery(playerPosition, attractionRadius, data);
            
            plantQueryRequested.Invoke(search);

            if (search.nearest != null)
            {
                spawnableSpecies.Add(data);
                totalWeight += data.spawnWeight;
            }
        }
        if (spawnableSpecies.Count == 0)
        {
            return null;
        }

        // then a weighted pick, same idea as the seed dropper's tickets in a hat,
        // except the "tickets" are each species' spawnWeight instead of a plant count
        float ticket = Random.Range(0f, totalWeight);
        foreach (animalData data in spawnableSpecies)
        {
            ticket -= data.spawnWeight;
            if (ticket < 0f)
            {
                return data;
            }
        }
        return spawnableSpecies[spawnableSpecies.Count - 1]; //float Range can return the max exactly, which lands here
    }

    // a random spot on a ring around the player, dropped onto the ground
    bool findSpawnPoint(out Vector3 spawnPoint)
    {
        for (int i = 0; i < spotAttempts; i++)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
            Vector3 above = playerPosition + new Vector3(direction.x, 0f, direction.y) * distance + Vector3.up * groundRayHeight;

            if (Physics.Raycast(above, Vector3.down, out RaycastHit hit, groundRayHeight * 2f, groundMask))
            {
                spawnPoint = hit.point;
                return true;
            }
        }
        spawnPoint = Vector3.zero;
        return false;
    }
}