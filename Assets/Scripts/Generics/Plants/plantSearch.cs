using UnityEngine;

public class plantQuery
{
    //used by animals or spawners to find near plants
    public Vector3 position;
    public float radius;
    public animalData data;

    public abstractPlant nearest; // null if nothing was found
    public Vector3 nearestPosition;
    public float nearestDistance = float.MaxValue;

    public plantQuery(Vector3 position, float radius, animalData data)
    {
        this.position = position;
        this.radius = radius;
        this.data = data;
    }
}