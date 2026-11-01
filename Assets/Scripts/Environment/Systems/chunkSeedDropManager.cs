using System.Collections.Generic;
using UnityEngine;

// every in-game hour there's a chance a seed drops somewhere in this chunk,
// and which seed it is depends on what's already growing in the chunk
// a chunk with nothing growing drops nothing 
public class chunkSeedDropManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    chunkController chunk;
    [SerializeField]
    seedPickup pickupPrefab;

    [Header("Drop Settings")]
    [SerializeField]
    [Range(0f, 1f)]
    float dropChancePerHour = 0.15f;
    [SerializeField]
    int maxSeedsInChunk = 3; //stops seeds piling up 
    [SerializeField]
    int spotAttempts = 5; //how many random spots to try before giving up
    [SerializeField]
    float dropHeightOffset = 0.1f;
    List<seedPickup> activePickups = new List<seedPickup>();

    // gets when the component is first added
    void OnEnable()
    {
        chunk = GetComponent<chunkController>();
    }

    public void onHourChange(int amount)
    {
        // pickups destroy themselves when collected, which leaves "null" entries in the list. clear those out before counting
        activePickups.RemoveAll(pickup => pickup == null);

        // one roll per hour that passed - skipping 3 hours at once still gives 3 chances
        for (int i = 0; i < amount; i++)
        {
            if (activePickups.Count >= maxSeedsInChunk)
            {
                return;
            }
            if (Random.value < dropChancePerHour)
            {
                tryDropSeed();
            }
        }
    }

    public bool tryDropSeed()
    {
        plantSeed seed = pickSeedFromChunk();
        if (seed == null)
        {
            return false; //nothing growing here, so nothing to drop
        }

        for (int i = 0; i < spotAttempts; i++)
        {
            Vector3 point = chunk.getRandomPosInChunk();
            if (chunk.getPlantInCell(chunk.getCellFromWorldPosition(point)) != null)
            {
                continue; //don't drop it inside a plant
            }

            seedPickup pickup = Instantiate(pickupPrefab, point + Vector3.up * dropHeightOffset, Quaternion.identity);

            pickup.setSeed(seed);
            activePickups.Add(pickup);

            Debug.Log($"Dropped a {seed.displayName} seed");
            return true;
        }
        return false;
    }

    public void dropSeed(int amount)
    {

        for (int i = 0; i < amount; i++)
        {
            tryDropSeed();
        }

    }


    plantSeed pickSeedFromChunk()
    {
        Dictionary<plantSeed, int> counts = chunk.getPlantCounts();

        int total = 0;
        foreach (int count in counts.Values)
        {
            total += count;
        }
        if (total == 0)
        {
            return null;
        }

        // every existing plant puts one ticket in the system, so a seed's share is its count over the total - this is not the best solution
        // I would rather have a weighted version but for now this wiull do
        int ticket = Random.Range(0, total); //the int version of Range never returns the max, so this is 0 to total-1
        foreach (var (seed, count) in counts)
        {
            ticket -= count;
            if (ticket < 0)
            {
                return seed;
            }
        }
        return null; //ackup
    }
}
