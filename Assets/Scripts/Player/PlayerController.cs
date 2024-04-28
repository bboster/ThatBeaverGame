using System;
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
    [SerializeField]
    ParticleSystem chompParticles;
    [SerializeField]
    ParticleSystem fallingParticles;
    [SerializeField]
    GameObject playerModel;

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
    [SerializeField]
    float maxSlopeAngle = 60;

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
    float minSpeedToStartWallRun = 4f;
    [SerializeField]
    float wallRunGravityMult = 0.2f;
    [SerializeField]
    float wallRunStickMult = 5;
    [SerializeField]
    float wallRunningSpeed = 5;
    [SerializeField]
    bool canRunOnPreviousWall = true;

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

    Vector3 wallRunSmoothing;

    float targetRotationAngle = 0;

    // Physics Assignments
    Rigidbody rb;

    PhysicMaterial physicMaterial;

    bool hasTouchedGrass = true;

    bool isTouchingGrass = true;

    bool isOnWall = false;

    bool gravityEnabled = true;

    // Slope Calculations
    RaycastHit slopeHit;

    // Cooldowns
    float dashCurrentCooldown = 0;
    float wallJumpCurrentCooldown = 0;
    float wallJumpRemainingDuration = 0;

    // Scaling Stats
    BeaverStats playerStats;

    Vector3 startScale;

    bool isModelInverted = false;

    public enum MovementState
    {
        STATIONARY,
        MOVING,
        DASHING,
        SLAMMING
    };

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.drag = airDrag;
        physicMaterial = GetComponent<Collider>().material;

        playerInput = GetComponent<PlayerInput>();
        playerInput.currentActionMap.Enable();

        anim = GetComponent<Animator>();

        playerStats = GetComponent<BeaverStats>();
        startScale = transform.localScale;

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
        if (movementState == MovementState.STATIONARY)
          return;

        if (isOnWall && !isTouchingGrass)
        {
            Vector3 wallForward = CalculateWallForward();
            if (wallForward != Vector3.zero)
            {
                transform.LookAt(transform.position + wallForward);
                //Quaternion targetRotation = Quaternion.LookRotation(wallForward);
                //transform.rotation = targetRotation;
                //transform.rotation = Quaternion.Euler(Vector3.SmoothDamp(transform.rotation.eulerAngles, targetRotation.eulerAngles, ref wallRunSmoothing, wallRunRotationTime));
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
        else if(movementState != MovementState.SLAMMING)
            movementState = MovementState.MOVING; anim.SetBool("isRunning", true);

        newVelocity = (Quaternion.Euler(0, targetRotationAngle, 0) * Vector3.forward).normalized;
        newVelocity *= moveSpeed;

        newVelocity = VectorUtils.ClampHorizontalVelocity(rb.velocity, newVelocity * playerStats.GetStat(ScalableStat.SPEED), 
            (!isTouchingGrass && !isOnWall ? airSpeedLimit : groundSpeedLimit) * playerStats.GetStat(ScalableStat.SPEED));

        if (IsOnSlope())
            rb.AddForce(GetSlopeMoveDirection() * newVelocity.magnitude, ForceMode.Acceleration);

        rb.AddForce(newVelocity, ForceMode.Acceleration);
        //Debug.Log("Speed: " + rb.velocity.magnitude);
    }

    private bool IsOnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, transform.localScale.y * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(Get3DMovement(), slopeHit.normal).normalized;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (!isTouchingGrass)
            return;

        if (wallJumpCurrentCooldown > 0)
            return;

        SetTouchedGrass(false);

        if (rb == null)
        {
            return;
        }

        rb.AddForce(Vector3.up * (jumpHeight * playerStats.GetStat(ScalableStat.JUMP_HEIGHT)), ForceMode.Impulse);
        anim.SetTrigger("jump");

        wallJumpCurrentCooldown = wallJumpCooldown;
    }

    private void WallJump(InputAction.CallbackContext context)
    {
        if (!isOnWall || isTouchingGrass)
            return;

        if (wallJumpCurrentCooldown > 0)
            return;

        if (anim == null)
        {
            return;
        }

        anim.SetTrigger("jump");

        rb.velocity = VectorUtils.ZeroOutYAxis(rb.velocity);

        Vector3 wallJumpForce = wallDetector.GetWallNormal().normalized * wallJumpHorizontalForce;

        wallJumpForce += Get3DMovement().normalized * wallJumpInputModifier;
        wallJumpForce *= playerStats.GetStat(ScalableStat.SPEED);

        wallJumpForce.y = wallJumpVerticalForce * playerStats.GetStat(ScalableStat.JUMP_HEIGHT);

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

        if (!hasTouchedGrass)
            return;

        if (movementState == MovementState.SLAMMING)
            return;

        if (anim == null)
        {
            return;
        }

        anim.SetTrigger("dash");

        StartCoroutine(DashOverTime());
    }

    private IEnumerator DashOverTime()
    {
        dashCurrentCooldown = dashCooldown;

        hasTouchedGrass = false;

        movementState = MovementState.DASHING;
        Vector3 dashVelocity;
        if (isOnWall)
            dashVelocity = transform.forward * dashSpeed;
        else
            dashVelocity = (Quaternion.Euler(0, targetRotationAngle, 0) * Vector3.forward).normalized * dashSpeed;

        dashVelocity *= playerStats.GetStat(ScalableStat.SPEED);
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

        if (movementState == MovementState.SLAMMING)
            return;

        if (wallRunningResetsDashCD && dashCurrentCooldown > 0)
        {
            dashCurrentCooldown = 0;
            hasTouchedGrass = true;
        }

        if(rb.velocity.magnitude < 0.05f)
        {
            SetTouchedWall(false);
            return;
        }

        Vector3 wallRunStickForce = wallDetector.GetWallNormal().normalized * -wallRunStickMult;

        rb.AddForce(wallRunStickForce, ForceMode.Force);

        Vector3 wallRunBoost = CalculateWallForward();
        if (wallRunBoost == Vector3.zero)
            return;

        wallRunBoost *= playerStats.GetStat(ScalableStat.SPEED);
        rb.AddForce(wallRunBoost * wallRunningSpeed, ForceMode.Acceleration);

        // Speedometer
        //Debug.Log(VectorUtils.ZeroOutYAxis(rb.velocity).magnitude);
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

        if (isTouchingGrass)
        {
            hasTouchedGrass = true;

            wallDetector.ResetStoredWalls();

            if (grassTouchResetsDashCD)
                dashCurrentCooldown = 0;
        }

        ToggleAirDrag(!touchedGrass);
    }

    public void SetTouchedWall(bool touchedWall)
    {
        if (touchedWall)
        {
            if (isTouchingGrass)
                return;

            if (wallJumpRemainingDuration > 0)
                return;

            if (VectorUtils.ZeroOutYAxis(rb.velocity).magnitude < minSpeedToStartWallRun)
                return;

            if (!canRunOnPreviousWall && wallDetector.IsOnPreviousWall())
                return;
        }

        isOnWall = touchedWall;

        anim.SetBool("wallrunning", touchedWall);

        SetModelInverted(wallDetector.IsWallLeft()); 


        if (rb.velocity.y < 0)
            rb.velocity = VectorUtils.ZeroOutYAxis(rb.velocity);
    }

    private void SetModelInverted(bool inverted)
    {
        if (isModelInverted == inverted)
            return;

        isModelInverted = inverted;

        if (inverted)
        {
            Vector3 invertedScale = playerModel.transform.localScale;
            invertedScale.x = -1 * Mathf.Abs(invertedScale.x);

            playerModel.transform.localScale = invertedScale;
        }
        else
        {
            Vector3 scale = playerModel.transform.localScale;
            scale.x = Mathf.Abs(scale.x);

            playerModel.transform.localScale = scale;
        }
        
        //Debug.Log("IsInverted: " + inverted);
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
        if (rb == null)
        {
            return;
        }

        rb.drag = isInAir ? airDrag : groundDrag;

        StartCoroutine(DelayedFrictionChange(isInAir));
    }

    IEnumerator DelayedFrictionChange(bool isInAir)
    {
        yield return new WaitForSeconds(0.2f);
        physicMaterial.dynamicFriction = isInAir ? 0.0f : 0.6f;
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

    public void SetMovementState(MovementState newState)
    {
        movementState = newState;
    }

    public MovementState GetMovementState()
    {
        return movementState;
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
    /// <summary>
    /// Simply plays chomp particle during chomp animation
    /// </summary>
    public void PlayChompParticle()
    {
        anim.ResetTrigger("chomp");
        chompParticles.Play();
    }
    /// <summary>
    /// Resets the trigger of any animation. Trigger defined by string input.
    /// </summary>
    public void ResetAnimTrigger(string triggerName)
    {
        anim.ResetTrigger(triggerName);
    }
    public void FallingParticleToggle(int toggle)
    {
        switch (toggle)
        {
            case 0:
                fallingParticles.Stop();
                break;
            case 1:
                fallingParticles.Play();
                break;
            default:
                Debug.Log("Error trying to read the fallingParticle toggle value!");
                break;
        }
    }

    public void UpdateScale()
    {
        transform.localScale = startScale * playerStats.GetStat(ScalableStat.SIZE);
    }
}
