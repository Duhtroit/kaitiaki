using UnityEngine;

public class placeholderPlant : abstractPlant
{

    public override float waterUseRate => 1f; //never seen this before right now, but this forces an implementation to create this
    public override float growthRate => 0.5f;
    public override float damageRate => 2f;
    public override float idealMoisture => 1.0f;
    public override float idealMoistureRange => 30f;

}
