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
    float turnSmoothTime = 0.1f;

    [SerializeField]
    float rotationOffset = 90;

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
    float wallRunGravityMult = 0.2f;

    [SerializeField]
    float fallingGravityMod;

    // Private Assignments

    MovementState movementState = MovementState.MOVING;

    // Input Assignments
    PlayerInput playerInput;

    InputAction movementAction;

    Vector2 moveInput;

    float turnSmoothVelocity;

    float targetRotationAngle = 0;

    // Physics Assignments
    Rigidbody rb;

    PhysicMaterial physicMaterial;

    bool isTouchingGrass = true;

    bool isOnWall = false;

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

        Debug.Log(transform.rotation.eulerAngles);
    }

    private void LateUpdate()
    {
        Look();
    }

    private void Look()
    {
        //if (movementState == MovementState.STATIONARY)
          //  return;

        Vector2 input = GetMoveInput();
        targetRotationAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;

        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotationAngle, ref turnSmoothVelocity, turnSmoothTime);

        Debug.Log("Rotation: " + angle);
        rb.rotation = Quaternion.Euler(new(0, angle, 0));
    }

    private void Move()
    {
        if (movementState == MovementState.DASHING)
            return;

        Vector3 newVelocity = Get3DMovement();
        if (newVelocity.magnitude == 0)
        {
            movementState = MovementState.STATIONARY;
            return;
        }
        else
            movementState = MovementState.MOVING;

        //newVelocity = moveSpeed * newVelocity.normalized;
        newVelocity = (Quaternion.Euler(0, targetRotationAngle, 0) * Vector3.forward).normalized;
        newVelocity *= moveSpeed;
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
        Vector3 dashVelocity = (Quaternion.Euler(0, targetRotationAngle, 0) * Vector3.forward).normalized * dashSpeed;

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
        gravMod *= isOnWall ? wallRunGravityMult : 1;
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
    public void SetTouchedGrass(bool touchedGrass)
    {
        isTouchingGrass = touchedGrass;

        ToggleAirDrag(!touchedGrass);
    }

    public void SetTouchedWall(bool touchedWall)
    {
        isOnWall = touchedWall;

        /*Vector3 currentVelocity = rb.velocity;
        if(currentVelocity.y < 0)
        {
            currentVelocity.y = 0;
            rb.velocity = currentVelocity;
        }*/
            
    }

    public bool IsGrounded()
    {
        return isTouchingGrass;
    }

    public bool IsOnWall()
    {
        return isOnWall;
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
