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

    [Header("Dash")]
    [SerializeField]
    float dashCooldown = 0.5f;
    [SerializeField]
    float dashSpeed = 20;
    [SerializeField]
    float dashDuration = 0.4f;

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

    MovementState movementState = MovementState.MOVING;

    // Input Assignments
    PlayerInput playerInput;

    InputAction movementAction;

    Vector2 moveInput;

    // Physics Assignments
    Rigidbody rb;

    PhysicMaterial physicMaterial;

    bool isTouchingGrass = true;

    bool gravityEnabled = true;

    // Dash Assignments
    float dashCurrentCooldown = 0;

    public enum MovementState
    {
        STATIONARY,
        MOVING,
        DASHING
    };

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.drag = airDrag;
        physicMaterial = GetComponent<Collider>().material;

        playerInput = GetComponent<PlayerInput>();
        playerInput.currentActionMap.Enable();

        movementAction = playerInput.currentActionMap.FindAction("Movement");
        playerInput.currentActionMap.FindAction("Jump").performed += Jump;
        playerInput.currentActionMap.FindAction("Dash").performed += Dash;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        Move();

        Gravity();
    }

    private void Update()
    {
        CooldownTick();
    }

    private void LateUpdate()
    {
        Look();
    }

    private void Look()
    {
        //if (movementState == MovementState.DASHING)
          //  return;

        Vector3 newRotation = new(0, cameraTransform.rotation.eulerAngles.y, 0);
        
        //Debug.Log("Rotation: " + newRotation);
        transform.rotation = Quaternion.Euler(newRotation);
    }

    private void Move()
    {
        if (movementState == MovementState.DASHING)
            return;

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

    public void Dash(InputAction.CallbackContext context)
    {
        if (dashCurrentCooldown > 0)
            return;

        StartCoroutine(DashOverTime());
    }

    private IEnumerator DashOverTime()
    {
        dashCurrentCooldown = dashCooldown;

        movementState = MovementState.DASHING;
        Vector3 dashVelocity = Get3DMovement() * dashSpeed;

        rb.velocity = dashVelocity;

        gravityEnabled = false;

        yield return new WaitForSeconds(dashDuration);

        gravityEnabled = true;
        movementState = MovementState.MOVING;
    }

    public void CooldownTick()
    {
        if (dashCurrentCooldown <= 0)
            return;

        dashCurrentCooldown -= Time.deltaTime;
    }

    private void Gravity()
    {
        if (!gravityEnabled)
            return;

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
