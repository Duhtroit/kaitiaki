using System.Collections.Generic;
using UnityEngine;

// the seeds the player is carrying, and which one is selected for planting.
public class playerInventory : MonoBehaviour
{
    [Header("Starting Seeds")]
    [SerializeField]
    List<plantSeed> startingSeeds = new List<plantSeed>(); //add the same seed twice to start with 2 of it

    Dictionary<plantSeed, int> seedCounts = new Dictionary<plantSeed, int>();


    // a dictionary doesn't keep order,list remembers the order seeds were first picked up in.
    List<plantSeed> seedOrder = new List<plantSeed>();
    int selectedIndex = 0;

    void Start()
    {
        foreach (plantSeed seed in startingSeeds)
        {
            addSeed(seed);
        }
    }

    // hooked up to the Seed Collected event
    public void addSeed(plantSeed seed)
    {
        if (seed == null)
        {
            return;
        }

        if (seedCounts.ContainsKey(seed))
        {
            seedCounts[seed]++;
        }
        else
        {
            seedCounts.Add(seed, 1);
            seedOrder.Add(seed);
        }

        Debug.Log($"Got a {seed.displayName} seed ({seedCounts[seed]} total)");
    }


    public void removeSeed(plantSeed seed)
    {
        if (seed == null || !seedCounts.ContainsKey(seed))
        {
            return;
        }

        seedCounts[seed]--;
        if (seedCounts[seed] > 0)
        {
            return;
        }

        // last one used up, so take it out of the cycle completely
        seedCounts.Remove(seed);
        int removedIndex = seedOrder.IndexOf(seed);
        seedOrder.RemoveAt(removedIndex);

        // removing from the list shifts everything after it back one spot.
        // if the selected seed was after the removed one, move the index back too so it stays on the same seed
        if (selectedIndex > removedIndex)
        {
            selectedIndex--;
        }
        if (selectedIndex >= seedOrder.Count)
        {
            selectedIndex = 0;
        }
    }

    public plantSeed getSelectedSeed()
    {
        return seedOrder.Count == 0 ? null : seedOrder[selectedIndex];
    }

    public int getSeedCount(plantSeed seed)
    {
        if (seed == null)
        {
            return 0;
        }
        return seedCounts.TryGetValue(seed, out int count) ? count : 0;
    }

    public void nextSeed()
    {
        if (seedOrder.Count == 0)
        {
            return;
        }
        selectedIndex = (selectedIndex + 1) % seedOrder.Count; //% wraps it back to 0 after the last one
        Debug.Log($"Selected {getSelectedSeed().displayName} ({getSeedCount(getSelectedSeed())})");
    }

    public void previousSeed()
    {
        if (seedOrder.Count == 0)
        {
            return;
        }
        // + Count before the % so going back from 0 wraps to the last seed (it became -1)
        selectedIndex = (selectedIndex - 1 + seedOrder.Count) % seedOrder.Count;
        Debug.Log($"Selected {getSelectedSeed().displayName} ({getSeedCount(getSelectedSeed())})");
    }
}
