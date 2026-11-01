using UnityEngine;

public class kawakawaPlant : abstractPlant
{
    public override float waterUseRate => 1f;
    public override float growthRate => 1f;

    public override float damageRate => 1f;
    public override float idealMoisture => 1f;

    public override float idealMoistureRange => 10f;

}