using System.Collections.Generic;
using UnityEngine;

// everything that's the same for every animal of one species
[CreateAssetMenu(fileName = "Animal Data", menuName = "Animals/Animal Data")]
public class animalData : ScriptableObject
{
    [Header("Identity")]
    public string displayName;
    public animal prefab;

    [Header("Spawning")]
    public float spawnWeight = 1f; //higher = picked more often by the spawner, compared to the other species

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float turnSpeed = 360f; //degrees per second
    public bool canFly = false;
    public float cruiseHeight = 4f; //how high flyers travel above the ground
    public float verticalSpeed = 3f; //how fast flyers climb and descend
    public float landingDistance = 3f; //flyers start coming down when they're this close to where they're going

    [Header("Senses")]
    public float senseRadius = 15f; //how far away it can notice plants

    [Header("Hunger")]
    // hunger goes from 0 (full) to 1 (starving)
    public Vector2 startHungerRange = new Vector2(0.5f, 1f);
    public float hungerPerSecond = 0.01f;
    [Range(0f, 1f)]
    public float hungryThreshold = 0.4f; //goes looking for food once hunger is above this

    [Header("Eating")]
    public List<plantSeed> edibleSeeds = new List<plantSeed>(); //leave empty to eat anything
    [Range(0f, 100f)]
    public float minPlantGrowthPercent = 25f; //ignores seedlings smaller than this
    public float eatDistance = 0.6f; //how close it gets to a plant before eating
    public float eatInterval = 1.5f; //seconds between bites
    public float eatAmount = 5f; //plant health taken per bite
    public float hungerPerBite = 0.2f; //how much hunger each bite takes away

    [Header("Wandering")]
    public float wanderRadius = 6f;
    public Vector2 idleTimeRange = new Vector2(2f, 5f);

    [Header("Leaving")]
    public bool leavesAfterEating = true;
    public float maxStaySeconds = 120f; //leaves after this long even if it never found food
    public float leaveDistance = 30f; //how far it goes before disappearing
}
