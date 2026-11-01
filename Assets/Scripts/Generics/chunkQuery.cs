using UnityEngine;


public class chunkQuery
{
    //used by events to ask about cells, and carry data pertaining to the cell back.
    public Vector3 point;

    public bool answered;
    public Vector3 cellPosition;
    public bool occupied;
    public abstractPlant plant;
    public npc npc;
    public string name;

    public chunkQuery(Vector3 point)
    {
        this.point = point;
    }
}
