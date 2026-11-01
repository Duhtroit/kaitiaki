using UnityEngine;
// sent by an animal when it takes a bite. the chunk that owns the plant takes the health off
public class plantEat
{
    public abstractPlant plant;
    public float amount;

    public plantEat(abstractPlant plant, float amount)
    {
        this.plant = plant;
        this.amount = amount;
    }
}
