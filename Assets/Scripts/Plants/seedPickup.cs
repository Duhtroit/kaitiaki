using UnityEngine;

public class seedPickup : MonoBehaviour
{
    [Header("Seed")]
    [SerializeField]
    plantSeed seed; //set by the chunkSeedDropper when it spawns this

    [Header("Events")]
    [SerializeField]
    seedEvent seedCollected;

    string playerTag = "Player"; //has to be on the same object as the player's collider

    bool collected = false;

    public void setSeed(plantSeed newSeed)
    {
        seed = newSeed;
    }

    void OnTriggerEnter(Collider other)
    {
        // collected stops collsion from firing twice 
        if (collected || seed == null)
        {
            return;
        }
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        collected = true;
        seedCollected.Invoke(seed);
        Destroy(gameObject);
    }
}
