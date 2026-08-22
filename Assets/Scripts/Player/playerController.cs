using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerController : MonoBehaviour
{
    [SerializeField]
    private CharacterController controller;
    [SerializeField]
    private float playerSpeed;
    [SerializeField]
    private float playerRotationSpeed;
    [SerializeField]
    private vector3Event onPlayerPositionChange;
    [SerializeField]
    private boolEvent onEnterPlantingMode;

    private Transform selfTransform;

    private bool plantingModeOn;

    private InputAction move;
    private Vector3 direction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        move = InputSystem.actions.FindAction("Move");
        selfTransform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        updatePosition();
    }

    void updatePosition()
    {
        if (move.IsPressed())
        {
            direction = new Vector3(move.ReadValue<Vector2>().x, 0, move.ReadValue<Vector2>().y);
            controller.Move(direction * Time.deltaTime * playerSpeed);
            onPlayerPositionChange.Invoke(selfTransform.position);
            selfTransform.forward = direction;

        }
    }
}
