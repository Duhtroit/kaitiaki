using UnityEngine;

// the numbers the animator's int
public enum animalAnimState
{
    idle = 0,
    moving = 1, //walking or flying
    eating = 2
}


public class animal : MonoBehaviour
{

    // the animal's data and "body" - its hunger, it's target, and how it moves/animations.

    [Header("Animal Settings")]
    [SerializeField]
    animalData data; //the spawner overrides this, but setting it on the prefab lets you test by dropping one in the scene
    [SerializeField]
    LayerMask groundMask; //just the chunk ground layer
    [SerializeField]
    float groundRayHeight = 50f;

    [Header("References")]
    [SerializeField]
    Animator animator; //on the model, a direct child of this object

    [Header("Events")]
    [SerializeField]
    plantEatEvent onPlantEat;
    [SerializeField]
    plantQueryEvent plantQueryRequested;

    [Header("Animal State")]
    // serialized only so you can watch them change in the inspector while testing
    [SerializeField]
    float hunger;
    [SerializeField]
    float timeAlive;
    [SerializeField]
    bool hasEaten;

    abstractPlant targetPlant;
    Vector3 targetPosition;
    Vector3 destination;
    float currentHeight; // how far above the ground it is right now (only ever above 0 for flyers)
    bool setupDone = false;


    // called by the spawner straight after it instantiates this
    public void setup(animalData newData)
    {
        data = newData;
        setupDone = true;

        hunger = Random.Range(data.startHungerRange.x, data.startHungerRange.y);
        timeAlive = 0f;
        hasEaten = false;

        // flyers arrive already in the air
        currentHeight = data.canFly ? data.cruiseHeight : 0f;
        Vector3 position = transform.position;
        position.y = getGroundHeight(position) + currentHeight;
        transform.position = position;

        setAnimState(animalAnimState.moving);
    }

    void Start()
    {
        // if nothing called setup (the animal was placed in the scene by hand) set it up with the prefab's data
        if (!setupDone && data != null)
        {
            setup(data);
        }
    }

    void Update()
    {
        if (data == null)
        {
            return; //not set up yet (no data on the prefab and no spawner)
        }

        hunger = Mathf.Min(1f, hunger + data.hungerPerSecond * Time.deltaTime);
        timeAlive += Time.deltaTime;
    }

    // hooked up to the PlantRemoved event - if our plant was eaten, removed by the player, or died, forget it
    public void onPlantRemoved(abstractPlant plant)
    {
        if (plant == targetPlant)
        {
            targetPlant = null;
        }
    }



    // ---------------- behavior graph  ----------------

    public bool isHungry()
    {

        return hunger >= data.hungryThreshold;
    }

    public bool wantsToLeave()
    {

        return (data.leavesAfterEating && hasEaten) || timeAlive >= data.maxStaySeconds;

    }

    // onPlantRemoved clears the target when the plant goes, so we only need to check it's still set
    public bool hasValidTarget()
    {
        return targetPlant != null;

    }



    // ---------------- things the behavior graph actions ----------------

    public bool foundPlant()
    {
        plantQuery search = new plantQuery(transform.position, data.senseRadius, data);

        plantQueryRequested.Invoke(search); //the chunks fill in the answer before this line finishes, so cool

        targetPlant = search.nearest;
        targetPosition = search.nearestPosition;

        return targetPlant != null;

    }

    public void clearTarget()
    {
        Debug.Log($"{data.displayName} clearTarget");

        targetPlant = null;
    }

    public void chooseWaypoint()
    {
        clearTarget();

        Vector2 offset = Random.insideUnitCircle * data.wanderRadius;
        destination = transform.position + new Vector3(offset.x, 0f, offset.y);


        Debug.Log($"{data.displayName} has chosen waypont {destination}");

    }

    public void startLeaving()
    {
        clearTarget();

        Vector2 direction = Random.insideUnitCircle.normalized;
        destination = transform.position + new Vector3(direction.x, 0f, direction.y) * data.leaveDistance;
        setAnimState(animalAnimState.moving);
    }

    public bool moveToTarget()
    {

        return moveTowards(targetPosition, data.eatDistance, true);

    }

    public bool moveToDestination(bool land)
    {
        return moveTowards(destination, 0.3f, land);

    }

    // takes one bite out of the target plant. returns true once the animal is full
    public bool bite()
    {
        if (!hasValidTarget())
        {
            Debug.Log($"{data.displayName} has no valid target");
            return false;

        }

        facePosition(targetPosition);

        onPlantEat.Invoke(new plantEat(targetPlant, data.eatAmount));

        hunger = Mathf.Max(0f, hunger - data.hungerPerBite);
        if (hunger <= 0f)
        {
            hasEaten = true;
            return true;
        }
        return false;
    }

    public void despawn()
    {
        Debug.Log($"{data.displayName} is despawning");
        Destroy(gameObject); //the spawner clears destroyed animals out of its list on the next hour
    }

    public void setAnimState(animalAnimState state)
    {
        if (animator != null)
        {
            animator.SetInteger("State", (int)state);
        }
    }



    // ---------------- movement ----------------

    // moves one frame's worth towards a point. returns true when it's arrived.
    // land = false keeps flyers in the air (used when leaving)
    public bool moveTowards(Vector3 point, float stopDistance, bool land)
    {
        Debug.Log($"{data.displayName} is movingTowards {point}");

        Vector3 position = transform.position;

        // work out the distance on the ground only (ignoring height), since height is handled separately below
        Vector3 flatOffset = point - position;
        flatOffset.y = 0f;
        float flatDistance = flatOffset.magnitude;
        float remaining = flatDistance - stopDistance;
        // a tiny bit of slack, because float maths can leave it at 0.30000001 when it should be exactly 0.3
        bool closeEnough = remaining <= 0.01f;

        if (!closeEnough)
        {
            // Min stops it overshooting: on the last frame it only moves the bit that's left
            float stepSize = Mathf.Min(data.moveSpeed * Time.deltaTime, remaining);
            position += flatOffset.normalized * stepSize;

            facePosition(point);

            // if this step covered everything that was left, it's arrived
            if (stepSize >= remaining)
            {
                closeEnough = true;
            }
        }

        // height: walkers stay at 0 flyers climb to cruise height while travelling,
        // and only come down once they're within landingDistance of a spot they're landing at
        float wantedHeight = 0f;
        if (data.canFly && (!land || flatDistance > data.landingDistance))
        {
            wantedHeight = data.cruiseHeight;
        }
        currentHeight = Mathf.MoveTowards(currentHeight, wantedHeight, data.verticalSpeed * Time.deltaTime);
        position.y = getGroundHeight(position) + currentHeight;

        transform.position = position;

        // a flyer that's landing hasn't really arrived until its feet are on the ground
        if (land)
        {
            return closeEnough && currentHeight <= 0.01f; // annoying...
        }
        return closeEnough;
    }


    void facePosition(Vector3 point)
    {
        Debug.Log($"{data.displayName} is facing {point}");

        Vector3 direction = point - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return; //already standing on it, so there's no direction to face
        }
        Quaternion wanted = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, wanted, data.turnSpeed * Time.deltaTime);
    }

    // fires a ray down from high above to find the ground under a point.
    // if there's no ground (off the edge of the world) it just keeps the current height
    float getGroundHeight(Vector3 position)
    {
        Vector3 rayStart = new Vector3(position.x, position.y + groundRayHeight, position.z);


        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRayHeight * 2f, groundMask))
        {
            return hit.point.y;
        }
        return transform.position.y - currentHeight;
    }



    public animalData getData()
    {
        return data;
    }
}