using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Logic Assignments")]
    [SerializeField]
    GroundDetection groundDetector;

    [SerializeField]
    WallDetection wallDetector;

    [SerializeField]
    Transform cameraTransform;

    [Header("Movement")]
    [SerializeField]
    float moveSpeed;

    [SerializeField]
    float jumpHeight;

    [SerializeField]
    float groundSpeedLimit;

    [SerializeField]
    float airSpeedLimit;

    [Header("Physics")]
    [SerializeField]
    float groundDrag;

    [SerializeField]
    float airDrag;

    [SerializeField]
    float baseGravity;

    [SerializeField]
    float fallingGravityMod;

    // Private Assignments

    // Input Assignments
    PlayerInput playerInput;
    InputAction movementAction;

    Vector2 moveInput;

    // Physics Assignments
    Rigidbody rb;

    PhysicMaterial physicMaterial;

    // Public for debugging
    public bool isTouchingGrass = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.drag = airDrag;
        physicMaterial = GetComponent<Collider>().material;

        playerInput = GetComponent<PlayerInput>();
        playerInput.currentActionMap.Enable();

        movementAction = playerInput.currentActionMap.FindAction("Movement");
        playerInput.currentActionMap.FindAction("Jump").performed += Jump;

        ToggleAirDrag(false);

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Look();
        Move();

        Gravity();
    }

    private void Look()
    {
        Vector3 newRotation = new(0, cameraTransform.rotation.eulerAngles.y, 0);
        //Debug.Log("Rotation: " + newRotation);
        transform.rotation = Quaternion.Euler(newRotation);
    }

    private void Move()
    {
        Vector3 newVelocity = Get3DMovement();
        newVelocity = moveSpeed * newVelocity.normalized;
        //Debug.Log(newVelocity);

        newVelocity = VectorUtils.ClampHorizontalVelocity(rb.velocity, newVelocity, (isTouchingGrass ? groundSpeedLimit : airSpeedLimit));

        rb.AddForce(newVelocity, ForceMode.Acceleration);
        //rb.AddRelativeForce(1000 * Time.fixedDeltaTime * newVelocity, ForceMode.Acceleration);
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (!isTouchingGrass)
            return;

        rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
    }

    private void Gravity()
    {
        float gravMod = rb.velocity.y < 0 ? fallingGravityMod : 1;
        rb.AddForce(baseGravity * gravMod * Vector3.down);
    }

    Vector2 GetMoveInput()
    {
        return movementAction.ReadValue<Vector2>().normalized;
    }

    private Vector3 Get3DMovement()
    {
        moveInput = GetMoveInput();

        return moveInput.x * transform.right + moveInput.y * transform.forward;
    }
    public void SetTouchedGround(bool touchedGrass)
    {
        isTouchingGrass = touchedGrass;

        ToggleAirDrag(!touchedGrass);
    }

    public bool IsGrounded()
    {
        return isTouchingGrass;
    }

    private void ToggleAirDrag(bool isInAir)
    {
        rb.drag = isInAir ? airDrag : groundDrag;

        StartCoroutine(DelayedFrictionChange(isInAir));
    }

    IEnumerator DelayedFrictionChange(bool isInAir)
    {
        yield return new WaitForSeconds(0.2f);
        physicMaterial.dynamicFriction = isInAir ? 0.0f : 0.6f;
        //Debug.Log(physicMaterial.dynamicFriction);
    }
}
