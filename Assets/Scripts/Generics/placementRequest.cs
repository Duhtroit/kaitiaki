using UnityEngine;

// what the player sends when they want to plant something
// the chunk that owns worldPosition decides if it's allowed
public class placementRequest
{
    public plantSeed seed;
    public Vector3 worldPosition;

    public placementRequest(plantSeed seed, Vector3 worldPosition)
    {
        this.seed = seed;
        this.worldPosition = worldPosition;
    }
}
