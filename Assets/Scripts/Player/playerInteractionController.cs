using UnityEngine;
using UnityEngine.InputSystem;

public class playerInteractionController : MonoBehaviour
{

    [Header("Interaction References")]

    [SerializeField]
    private playerInventory inventory;
    [SerializeField]
    private Renderer cellMarker;
    [SerializeField]
    private Animator animator;

    [Header("Events")]
    [SerializeField]
    private npcInteractionEvent npcInteracted;
    [SerializeField]
    private placementRequestEvent placeRequested;
    [SerializeField]
    private plantEvent removeRequested;
    [SerializeField]
    private boolEvent onInteraction;
    [SerializeField]
    private stringEvent onInteractionHint;
    [SerializeField]
    private boolEvent onInteractionPossible;
    [SerializeField]
    private chunkQueryEvent chunkQueried;

    [Header("Targeting Settings")]
    [SerializeField]
    private float reach = 1.5f;
    [SerializeField]
    private float rayHeight = 3f;
    [SerializeField]
    private LayerMask groundMask;
    [SerializeField]
    private float markerHeightOffset = 0.02f;
    [SerializeField]
    private float markerSmoothing = 5f;

    [Header("Marker Colours")]
    [SerializeField]
    private Color canPlaceColour;
    [SerializeField]
    private Color canRemoveColour;
    [SerializeField]
    private Color unavailableColour;

    private InputAction interactAction;
    private InputAction nextAction;
    private InputAction previousAction;

    private chunkQuery query; //the chunk's answer about the cell in front of the player
    private bool hasTarget = false; //true when a chunk answered
    private Vector3 targetPoint;
    private abstractPlant targetPlant;
    private npc targetNPC;

    private enum pendingAction { none, place, remove, kneel }

    private pendingAction pending = pendingAction.none;

    private plantSeed pendingSeed;
    private Vector3 pendingLocation;
    private abstractPlant pendingPlant;
    private npc pendingNPC;

    private bool hintShowing = false;
    private string hintText;


    private bool isInteracting = false;
    private bool actionDone = false;

    private float interactTimeout = 2f;
    private float interactTimer;

    private void OnEnable()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
        nextAction = InputSystem.actions.FindAction("Next");
        previousAction = InputSystem.actions.FindAction("Previous");

    }

    private void Update()
    {
        if (!isInteracting)
        {
            findTarget();
        }
        else
        {
            interactTimer -= Time.deltaTime;

            if (interactTimer <= 0f)
            {
                endInteraction();
            }
        }

        // Update the target and marker as the player moves.
        updateMarkerAndUI();

        if (!isInteracting &&
            interactAction.WasPressedThisFrame())
        {
            tryInteraction();
        }

        if (!isInteracting &&
            nextAction.WasPressedThisFrame())
        {
            nextSeed();
        }

        if (!isInteracting&&
            previousAction.WasPressedThisFrame())
        {
            previousSeed();
        }
    }

    private void tryInteraction()
    {

        if (targetNPC != null) //npc is least likely so this is checked first, we should kneel
        {
            pending = pendingAction.kneel;
            pendingNPC = targetNPC;
        }
        //if there's 's a plant, we will remove it on interaction
        else if (targetPlant != null)
        {
            pending = pendingAction.remove;
            pendingPlant = targetPlant;
        }
        //if theres a plant in the inventory, we will place it on interaction
        else if (hasTarget && inventory.getSelectedSeed() != null)
        {
            pending = pendingAction.place;
            pendingSeed = inventory.getSelectedSeed();
            pendingLocation = targetPoint;
        }
        else
        {
            return;
        }

        isInteracting = true;
        actionDone = false;
        interactTimer = interactTimeout;

        // This will call onInteractionAnimation through animation events.

        switch (pending)
        {
            case (pendingAction.kneel):
                animator.SetTrigger("Kneel");
                break;
            default:
                animator.SetTrigger("Interact");
                break;
        }

    }

    public void onInteractionAnimation(bool value)
    {
        onInteraction.Invoke(value);

        // The true case is handled earlier.
        if (value == false)
        {
            endInteraction();
        }
    }

    public void onInteractionAnimationMidPoint()
    {
        if (!isInteracting || actionDone) return;

        actionDone = true;

        if (pending == pendingAction.place)
        {
            placeRequested.Invoke(new placementRequest(pendingSeed, pendingLocation)
            );
        }
        else if (pending == pendingAction.remove)
        {
            removeRequested.Invoke(pendingPlant);
        }
        else if (pending == pendingAction.kneel)
        {
            npcInteracted.Invoke(new npcInteraction(pendingNPC, transform));
        }
    }

    private void endInteraction()
    {
        if (!isInteracting) return;

        isInteracting = false;
        pending = pendingAction.none;
        pendingPlant = null;
        pendingSeed = null;
        pendingNPC = null;

        animator.ResetTrigger("Interact");
        animator.ResetTrigger("Kneel");
    }

    public void nextSeed()
    {
        if (inventory != null)
        {
            inventory.nextSeed();
        }
    }

    public void previousSeed()
    {
        if (inventory != null)
        {
            inventory.previousSeed();
        }
    }

    private void findTarget()
    {
        hasTarget = false;
        targetPlant = null;
        targetNPC = null;

        // Flatten forward so model tilt does not affect targeting height.
        Vector3 forward = transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
        {
            return;
        }

        forward.Normalize();

        Vector3 pointInFront = transform.position + forward * reach;

        // Ray straight down to find the ground.
        if (!Physics.Raycast(
                pointInFront + Vector3.up * rayHeight,
                Vector3.down,
                out RaycastHit hit,
                rayHeight * 2f,
                groundMask))
        {
            return;
        }

        targetPoint = hit.point;

        // ask the chunks what's here - the chunk that owns this point fills in the answer before this line finishes
        query = new chunkQuery(hit.point);
        chunkQueried.Invoke(query);

        if (!query.answered)
        {
            return; //this ground doesn't belong to any chunk
        }

        hasTarget = true;
        targetPlant = query.plant;
        targetNPC = query.npc;
    }

    private void updateMarkerAndUI()
    {
        bool canInteract = false;
        string hint = "";
        Color markerColour = unavailableColour;


        if (targetNPC != null) //npc target
        {
            canInteract = true;

            hint = $"Talk to {query.name}";
            markerColour = canPlaceColour;
        }

        else if (targetPlant != null) //plant targeted
        {
            canInteract = true;

            hint = $"Remove {query.name}";
            markerColour = canRemoveColour;
        }
        else if (hasTarget //check for inventory availabiltiy
         && inventory.getSelectedSeed() != null
                 && !query.occupied)
        {
            canInteract = true;

            hint = $"Plant {inventory.getSelectedSeed().displayName}";
            markerColour = canPlaceColour;
        }

        // nothing can be started mid-animation, so no hint while interacting
        if (isInteracting)
        {
            canInteract = false;
        }

        moveMarker(hasTarget, markerColour);
        setHint(canInteract, hint);

    }

    void moveMarker(bool show, Color colour)
    {
        cellMarker.gameObject.SetActive(show);
        if (!show)
        {
            return;
        }

        Vector3 targetPos = query.cellPosition;

        targetPos.y = targetPoint.y + markerHeightOffset;
        cellMarker.transform.position = Vector3.Lerp(cellMarker.transform.position, targetPos, markerSmoothing * Time.deltaTime);

        cellMarker.material.color = colour;
    }

    void setHint(bool show, string text) //changed this from being updated every frame
    {
        if (show && text != hintText)
        {
            hintText = text;
            onInteractionHint.Invoke(text);
        }
        if (show != hintShowing)
        {
            hintShowing = show;
            onInteractionPossible.Invoke(show);
        }
    }

}