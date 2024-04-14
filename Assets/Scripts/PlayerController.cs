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
    PlayerInput playerInput;

    // Input Assignments


    Vector2 moveInput;

    // Physics Assignments
    Rigidbody rb;

    bool isTouchingGrass = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        playerInput = GetComponent<PlayerInput>();
        playerInput.currentActionMap.Enable();

        Cursor.lockState = CursorLockMode.Locked;

        rb.drag = airDrag;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
