using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkForce = 20f;
    public float drag = 5f;
    
    [Header("Rotation")]
    public float rotationSpeed = 10f;
    
    
    [Header("Jump")]
    public float jumpForce = 7f;
    public float coyoteTime = 0.5f;
    public float jumpBufferTime = 0.2f;

    [Header("Dash")]
    public float dashDuration = .5f;
    public float dashDistance = 5f;
    public float dashHeight = 2f;
    
    [Header("Climb")]
    public float climbSpeed = 5f;
    
    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded, isTouchingWall, isClimbing, isDashing;
    
    public float coyoteTimer;
    public float jumpBufferTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void OnEnable()
    {
        //Movement
        InputHandler.OnMove += HandleMove;
        InputHandler.OnJump += HandleJumpInput;
        InputHandler.OnDash += HandleDashInput;
        
        //Ground and Wall Check
        GroundChecker.OnGroundEnter += OnGroundEnter;
        GroundChecker.OnGroundExit += OnGroundExit;
        WallChecker.OnWallEnter += OnWallEnter;
        WallChecker.OnWallExit += OnWallExit;
    }

    private void OnDisable()
    {
        //Movement
        InputHandler.OnMove -= HandleMove;
        InputHandler.OnJump -= HandleJumpInput;
        InputHandler.OnDash -= HandleDashInput;
        
        //Ground and Wall Check
        GroundChecker.OnGroundEnter -= OnGroundEnter;
        GroundChecker.OnGroundExit -= OnGroundExit;
        WallChecker.OnWallEnter -= OnWallEnter;
        WallChecker.OnWallExit -= OnWallExit;
    }

    private void FixedUpdate()
    {
        if (!isGrounded)
            coyoteTimer -= Time.fixedDeltaTime;
        jumpBufferTimer -= Time.fixedDeltaTime;
        
        if(isDashing) return;

        if (isClimbing)
        {
            rb.useGravity = false;
            Vector3 climbVelocity = Vector3.up * (moveInput.y * climbSpeed);
            rb.linearVelocity = new Vector3(0f, climbVelocity.y, 0f);
            return;
        }
        else
        {
            rb.useGravity = true;
        }
        
        Vector3 desiredDir = new Vector3(moveInput.x, 0f, moveInput.y);

        if (desiredDir.sqrMagnitude > 0.01f)
        {
            // --- Rotación suave hacia desiredDir ---
            Quaternion targetRot = Quaternion.LookRotation(desiredDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.fixedDeltaTime
            );

            // --- Aplicar fuerza en esa dirección ---
            rb.AddForce(
                desiredDir.normalized * walkForce,
                ForceMode.Acceleration
            );
        }

        // Drag horizontal (igual que antes)
        Vector3 horizVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(-horizVel * drag * Time.fixedDeltaTime, ForceMode.VelocityChange);

        if (jumpBufferTimer > 0 && (isGrounded || coyoteTimer > 0))
        {
            PerformJump();
            jumpBufferTimer = 0f;
        }
    }

    private void HandleMove(Vector2 input)
    {
        moveInput = input;
        //Debug.Log($"[MovementController] HandleMove → {moveInput}");
    }

    private void HandleJumpInput()
    {
        jumpBufferTimer = jumpBufferTime;
    }

    private void HandleDashInput()
    {
        if (!isDashing && !isClimbing)
        {
            StartCoroutine(PerformDash());
        }
    }

    private void OnGroundEnter()
    {
        isGrounded = true;
        if (jumpBufferTimer > 0f)
        {
            PerformJump();
            jumpBufferTimer = 0f;
        }
    }

    private void OnGroundExit()
    {
        isGrounded = false;
        coyoteTimer = coyoteTime;
    }

    private void OnWallEnter()
    {
        isTouchingWall = true;
        isClimbing = true;
    }

    private void OnWallExit()
    {
        isTouchingWall = false;
        isClimbing = false;
    }

    private void PerformJump()
    {
        rb.useGravity = true;            
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            jumpForce,
            rb.linearVelocity.z
        );
        isGrounded   = false;
        coyoteTimer  = 0f;   
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        float elapsedTime = 0;
        Vector3 startPos = transform.position;
        Vector3 direction = transform.forward;

        while (elapsedTime < dashDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dashDuration;
            
            float height = 4f * dashHeight * t * (1 - t);
            Vector3 targetPos = startPos + direction * dashDistance * t + Vector3.up * height;
            
            rb.MovePosition(targetPos);
            
            yield return null;
        }
        
        isDashing = false;
    }
}
