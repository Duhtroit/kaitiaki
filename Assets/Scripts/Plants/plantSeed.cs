using UnityEngine;

// one of these per plant type. it's the "id" for a type of plant - what the player carries round.
[CreateAssetMenu(fileName = "Plant Seed", menuName = "Plants/Plant Seed")]
public class plantSeed : ScriptableObject
{
    public string displayName;
    public GameObject plantPrefab; //what gets spawned when this seed is planted
}
