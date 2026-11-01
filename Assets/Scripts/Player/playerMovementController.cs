using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerMovementController : MonoBehaviour
{
    private enum animationAction { }

    [Header("Movement")]
    [SerializeField]
    private CharacterController controller;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private float runSpeed = 6f;
    [SerializeField]
    private float rotationDegreesPerSecond = 540f;
    [SerializeField]
    private float acceleration = 4f;
    [SerializeField]
    private float deceleration = 8f;

    [Header("Events")]
    [SerializeField]
    private vector3Event onPlayerPositionChange;
    [SerializeField]

    private transformEvent onPlayerPositionChange_transform;

    private float currentSpeed;
    private InputAction moveAction;
    private bool canMove = true;

    private void OnEnable()
    {
        moveAction = InputSystem.actions.FindAction("Move");

    }

    private void Update()
    {
        if (canMove)
        {
            handleMovement();

        }

    }

    private void handleMovement()
    {
        Vector2 input = Vector2.zero;

        input = moveAction.ReadValue<Vector2>();


        Vector3 direction = new Vector3(input.x, 0f, input.y);
        direction = Vector3.ClampMagnitude(direction, 1f);

        bool isMoving = direction.sqrMagnitude > 0.001f;

        if (isMoving)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                runSpeed,
                acceleration * Time.deltaTime
            );


            Quaternion targetRotation = Quaternion.LookRotation(direction);



            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationDegreesPerSecond * Time.deltaTime
            );

            controller.Move(direction * currentSpeed * Time.deltaTime);
            animator.SetFloat("Speed", currentSpeed);
        }
        else
        {

            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                deceleration * Time.deltaTime
            );

            animator.SetFloat("Speed", currentSpeed);
        }

        onPlayerPositionChange.Invoke(transform.position);
        onPlayerPositionChange_transform.Invoke(transform);
    }

    public void onInteractionAnimation(bool state)
    {
        canMove = !state;
    }
}
