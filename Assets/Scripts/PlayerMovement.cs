using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    public Camera playerCamera;
    public float walkSpeedCap = 6f;
    public float walkSpeedAccelerationMultiplier = 2f;
    public float runMultiplier = 1.5f;
    public float dragMultiplier = 0.01f;
    public float midAirDragMultiplier = 0.5f;
    public float midAirStrafeMultiplier = 0.25f;
    public float midAirStrafeRotationSpeed = 1f;
    public float midAirStrafeRotationCap = 45f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;
    public float cameraFov = 87f;
    private CharacterController characterController;

    private float rotationX = 0;
    private bool canMove = true;

    [SerializeField]
    private Vector3 moveDirection = Vector3.zero;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();

        PlayerInputManager.Instance.JumpEvent += HandleJump;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleGravity();
        if (canMove)
        {
            HandleMove();
            HandleCamera();
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleMove()
    {
        Vector3 move = PlayerInputManager.Instance.GetNormalizedMovement();

        // Player is mid air
        if (!characterController.isGrounded)
        {
            HandleMoveMidAir(move);
            return;
        }

        // Player isn't moving, drag (drift)
        if (move == Vector3.zero)
        {
            HandleDrag();
            return;
        }

        // Sprint forward
        if (PlayerInputManager.Instance.IsSprinting)
            move.z *= runMultiplier;

        // Move direction dependent on pitch of player
        move = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * move;

        // Drag movement if trying to go the opposite direction
        if (Mathf.Sign(moveDirection.x) != Mathf.Sign(move.x))
        {
            moveDirection.x *= (float)Math.Pow(dragMultiplier, Time.deltaTime);
        }
        if (Mathf.Sign(moveDirection.z) != Mathf.Sign(move.z))
        {
            moveDirection.z *= (float)Math.Pow(dragMultiplier, Time.deltaTime);
        }

        Vector3 walkSpeedCapDirection =
            new Vector3(moveDirection.x, 0, moveDirection.z).normalized * walkSpeedCap;

        // Update move direction
        Vector3 inputDelta =
            new Vector3(move.x, 0f, move.z)
            * walkSpeedCap
            * walkSpeedAccelerationMultiplier
            * Time.deltaTime;
        Vector3 newDirection = moveDirection + inputDelta;
        Vector2 flatDirection = new Vector2(newDirection.x, newDirection.z);

        // Check alignment between movement and facing if sprinting
        if (PlayerInputManager.Instance.IsSprinting)
        {
            Vector3 horizontalFacing = new Vector3(
                transform.forward.x,
                0f,
                transform.forward.z
            ).normalized;
            Vector3 horizontalMoveDir = new Vector3(move.x, 0f, move.z).normalized;

            float alignment = Vector3.Dot(horizontalFacing, horizontalMoveDir); // 1 = same direction, -1 = opposite

            // Extend the cap based on alignment
            float dynamicCap = Mathf.Lerp(
                walkSpeedCap,
                walkSpeedCap * runMultiplier,
                Mathf.Clamp01(alignment)
            );

            // Clamp movement
            flatDirection = Vector2.ClampMagnitude(flatDirection, dynamicCap);
        }
        else
        {
            // Clamp movement
            flatDirection = Vector2.ClampMagnitude(flatDirection, walkSpeedCap);
        }

        // Apply result
        moveDirection.x = flatDirection.x;
        moveDirection.z = flatDirection.y;
    }

    private void HandleMoveMidAir(Vector3 move)
    {
        // Sprint forward
        if (PlayerInputManager.Instance.IsSprinting)
            move.z *= runMultiplier;

        // Move direction dependent on pitch of player
        move = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * move;

        // Reduce overall speed, maintain it if moving that direction
        float midAirMultiplierX = Mathf.Lerp(
            midAirDragMultiplier,
            1f,
            float.IsNaN(move.x / moveDirection.x)
                ? 0
                : move.x * walkSpeedCap * walkSpeedAccelerationMultiplier / moveDirection.x
        );
        float midAirMultiplierZ = Mathf.Lerp(
            midAirDragMultiplier,
            1f,
            float.IsNaN(move.z / moveDirection.z)
                ? 0
                : move.z * walkSpeedCap * walkSpeedAccelerationMultiplier / moveDirection.z
        );

        // Update move direction
        moveDirection.x *= (float)Math.Pow(midAirMultiplierX, Time.deltaTime);
        moveDirection.z *= (float)Math.Pow(midAirMultiplierZ, Time.deltaTime);

        /// Add some mid-air strafing
        moveDirection.x +=
            move.x
            * walkSpeedCap
            * walkSpeedAccelerationMultiplier
            * midAirStrafeMultiplier
            * Time.deltaTime;
        moveDirection.z +=
            move.z
            * walkSpeedCap
            * walkSpeedAccelerationMultiplier
            * midAirStrafeMultiplier
            * Time.deltaTime;

        Vector2 currentDirection = new Vector2(moveDirection.x, moveDirection.z).normalized;
        Vector2 targetDirection = new Vector2(move.x, move.z).normalized;
        if (midAirStrafeRotationCap > Vector2.Angle(currentDirection, targetDirection))
        {
            float currentAngle = Vector2.SignedAngle(Vector2.up, currentDirection);
            float targetAngle = Vector2.SignedAngle(Vector2.up, targetDirection);

            moveDirection =
                (Mathf.DeltaAngle(currentAngle, targetAngle) > 0)
                    ? Quaternion.AngleAxis(-midAirStrafeRotationCap * Time.deltaTime, Vector3.up)
                        * moveDirection
                    : Quaternion.AngleAxis(midAirStrafeRotationCap * Time.deltaTime, Vector3.up)
                        * moveDirection;
        }
    }

    private void HandleDrag()
    {
        moveDirection.x *= (float)Math.Pow(dragMultiplier, Time.deltaTime);
        moveDirection.z *= (float)Math.Pow(dragMultiplier, Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (characterController.isGrounded)
            return;

        moveDirection.y -= gravity * Time.deltaTime;
    }

    private void HandleJump()
    {
        if (!characterController.isGrounded || !canMove)
            return;

        moveDirection.y = jumpPower;
    }

    private void HandleCamera()
    {
        // Up-down w/ clamping
        rotationX += -PlayerInputManager.Instance.GetMouseDeltaY() * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        // Left-right
        transform.rotation *= Quaternion.Euler(
            0,
            PlayerInputManager.Instance.GetMouseDeltaX() * lookSpeed,
            0
        );

        // Change fov if going fast
        playerCamera.fieldOfView =
            cameraFov
            * Mathf.Lerp(
                1f,
                1.2f,
                new Vector2(moveDirection.x, moveDirection.z).magnitude
                    / (walkSpeedCap * runMultiplier)
                    - 0.8f
            );
    }

    private void ToggleMovement()
    {
        canMove = !canMove;
        if (canMove)
        {
            PlayerInputManager.Instance.JumpEvent += HandleJump;
            return;
        }

        PlayerInputManager.Instance.JumpEvent -= HandleJump;
    }
}
