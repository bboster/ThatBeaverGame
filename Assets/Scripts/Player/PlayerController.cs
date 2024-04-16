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

    [Header("Visual Assignments")]
    [SerializeField]
    ParticleSystem dashParticles;
    [SerializeField]
    ParticleSystem runningParticles;

    [Header("Movement")]
    [SerializeField]
    float turnSmoothTime = 0.1f;
    [Space]
    [SerializeField]
    float moveSpeed;
    [SerializeField]
    float jumpHeight;
    [Space]
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
    [Space]
    [SerializeField]
    bool grassTouchResetsDashCD = true;
    [SerializeField]
    bool wallRunningResetsDashCD = true;

    [Header("Wall Running")]
    //[SerializeField]
    //float wallRunRotationTime = 0.025f;
    [SerializeField]
    float wallRunGravityMult = 0.2f;
    [SerializeField]
    float wallRunStickMult = 5;
    [SerializeField]
    float wallRunningSpeed = 5;

    [Header("Wall Jumping")]
    [SerializeField]
    float wallJumpInputModifier = 0.3f;
    [Space]
    [SerializeField]
    float wallJumpCooldown = 0.5f;
    [SerializeField]
    float wallJumpDuration = 0.33f;
    [Space]
    [SerializeField]
    float wallJumpVerticalForce = 5;
    [SerializeField]
    float wallJumpHorizontalForce = 5;

    [Header("Physics")]
    [SerializeField]
    float groundDrag;
    [SerializeField]
    float airDrag;
    [Space]
    [SerializeField]
    float baseGravity;
    [SerializeField]
    float fallingGravityMod;

    [Header("Animation")]
    public Animator anim;

    // Private Assignments

    MovementState movementState = MovementState.MOVING;

    // Input Assignments
    PlayerInput playerInput;

    InputAction movementAction;

    Vector2 moveInput;

    float turnSmoothVelocity;

    //Vector3 wallRunSmoothing;

    float targetRotationAngle = 0;

    // Physics Assignments
    Rigidbody rb;

    PhysicMaterial physicMaterial;

    bool isTouchingGrass = true;

    bool isOnWall = false;

    bool gravityEnabled = true;

    // Cooldowns
    float dashCurrentCooldown = 0;
    float wallJumpCurrentCooldown = 0;
    float wallJumpRemainingDuration = 0;

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

        anim = GetComponent<Animator>();

        movementAction = playerInput.currentActionMap.FindAction("Movement");
        playerInput.currentActionMap.FindAction("Jump").performed += Jump;
        playerInput.currentActionMap.FindAction("Jump").performed += WallJump;
        playerInput.currentActionMap.FindAction("Dash").performed += Dash;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDisable()
    {
        if (playerInput == null || playerInput.currentActionMap == null)
            return;

        playerInput.currentActionMap.FindAction("Jump").performed -= Jump;
        playerInput.currentActionMap.FindAction("Jump").performed -= WallJump;
        playerInput.currentActionMap.FindAction("Dash").performed -= Dash;
    }

    private void FixedUpdate()
    {
        Move();

        Gravity();

        WallRunning();
    }

    private void Update()
    {
        CooldownTick();
        anim.SetBool("isGrounded", isTouchingGrass);
    }

    private void LateUpdate()
    {
        Look();
    }

    private void Look()
    {
        //if (movementState == MovementState.STATIONARY)
        //  return;

        if (isOnWall && !isTouchingGrass)
        {
            Vector3 wallForward = CalculateWallForward();
            if (wallForward != Vector3.zero)
            {
                transform.LookAt(transform.position + wallForward);
                
                return;
            }
        }

        Vector2 input = GetMoveInput();
        targetRotationAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;

        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotationAngle, ref turnSmoothVelocity, turnSmoothTime);

        //Debug.Log("Rotation: " + angle);
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
            anim.SetBool("isRunning", false);
            return;
        }
        else
            movementState = MovementState.MOVING; anim.SetBool("isRunning", true);

        //newVelocity = moveSpeed * newVelocity.normalized;
        newVelocity = (Quaternion.Euler(0, targetRotationAngle, 0) * Vector3.forward).normalized;
        newVelocity *= moveSpeed;
        //Debug.Log(newVelocity);

        newVelocity = VectorUtils.ClampHorizontalVelocity(rb.velocity, newVelocity, (!isTouchingGrass && !isOnWall ? airSpeedLimit : groundSpeedLimit));

        rb.AddForce(newVelocity, ForceMode.Acceleration);
        //rb.AddRelativeForce(1000 * Time.fixedDeltaTime * newVelocity, ForceMode.Acceleration);
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (!isTouchingGrass)
            return;

        SetTouchedGrass(false);
        rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        anim.SetTrigger("jump");
    }

    private void WallJump(InputAction.CallbackContext context)
    {
        if (!isOnWall || isTouchingGrass)
            return;

        if (wallJumpCurrentCooldown > 0)
            return;

        Vector3 wallJumpForce = wallDetector.GetWallNormal().normalized * wallJumpHorizontalForce;
        wallJumpForce.y = wallJumpVerticalForce;

        rb.velocity = VectorUtils.ZeroOutYAxis(rb.velocity);

        wallJumpForce += Get3DMovement().normalized * wallJumpInputModifier;

        rb.AddForce(wallJumpForce, ForceMode.Impulse);

        SetTouchedWall(false);
        wallJumpRemainingDuration = wallJumpDuration;

        //Debug.Log("WALL JUMP");
        wallJumpCurrentCooldown = wallJumpCooldown;
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
        Vector3 dashVelocity;
        if (isOnWall)
            dashVelocity = transform.forward * dashSpeed;
        else
            dashVelocity = (Quaternion.Euler(0, targetRotationAngle, 0) * Vector3.forward).normalized * dashSpeed;


        rb.velocity = dashVelocity;

        gravityEnabled = false;

        dashParticles.Play();

        yield return new WaitForSeconds(dashDuration);

        gravityEnabled = true;
        movementState = MovementState.MOVING;
    }

    public void CooldownTick()
    {
        if (dashCurrentCooldown > 0)
            dashCurrentCooldown -= Time.deltaTime;

        if(wallJumpCurrentCooldown > 0)
            wallJumpCurrentCooldown -= Time.deltaTime;

        if (wallJumpRemainingDuration > 0)
            wallJumpRemainingDuration -= Time.deltaTime;
    }

    private void Gravity()
    {
        if (!gravityEnabled)
            return;

        float gravMod = rb.velocity.y < 0 ? fallingGravityMod : 1;
        gravMod *= isOnWall && !isTouchingGrass ? wallRunGravityMult : 1;

        rb.AddForce(baseGravity * gravMod * Vector3.down);
    }

    private void WallRunning()
    {
        if (!isOnWall || isTouchingGrass)
            return;

        if (wallRunningResetsDashCD && dashCurrentCooldown > 0)
            dashCurrentCooldown = 0;

        Vector3 wallRunStickForce = wallDetector.GetWallNormal().normalized * -wallRunStickMult;

        /*if (!wallDetector.IsWallLeft())
            wallRunStickForce *= -1;*/

        rb.AddForce(wallRunStickForce, ForceMode.Force);

        Vector3 wallRunBoost = CalculateWallForward();
        if (wallRunBoost == Vector3.zero)
            return;

        rb.AddForce(wallRunBoost * wallRunningSpeed, ForceMode.Acceleration);
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

        if (isTouchingGrass && grassTouchResetsDashCD)
            dashCurrentCooldown = 0;

        ToggleAirDrag(!touchedGrass);
    }

    public void SetTouchedWall(bool touchedWall)
    {
        if (wallJumpRemainingDuration > 0)
            return;

        isOnWall = touchedWall;

        if(rb.velocity.y < 0)
            rb.velocity = VectorUtils.ZeroOutYAxis(rb.velocity);

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

    private Vector3 CalculateWallForward()
    {
        Vector3 wallNormal = wallDetector.GetWallNormal();
        if (wallNormal == Vector3.zero)
            return Vector3.zero;
        
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((transform.forward - wallForward).magnitude > (transform.forward - -wallForward).magnitude)
            wallForward *= -1;

        return wallForward;
    }
    //The following functions are to be used as animation events during certain animations.

    /// <summary>
    /// Simply handles whether to turn runningParticles on or off. For some reason putting it in the other spot caused issues.
    /// </summary>
    public void RunningParticleToggle(int toggle)
    {
        switch (toggle)
        {
            case 0:
                runningParticles.Stop();
                break;
            case 1:
                runningParticles.Play();
                break;
            default:
                Debug.Log("Error trying to read the runningParticle toggle value!");
                break;
        }
    }
}
