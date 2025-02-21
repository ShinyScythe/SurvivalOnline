using NUnit.Framework;
using PurrNet;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float sprintSpeed = 4.5f;
    [SerializeField] private float jumpForce = 1f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.2f;

    [Header("Look Settings")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("References")]
    [SerializeField] private Vector3 cameraOffset;
    [SerializeField] private NetworkAnimator playerAnimator;
    [SerializeField] private List<Renderer> playerRenderers;
    [SerializeField] private Transform itemPoint;
    [SerializeField] private Transform rightHand;
    [SerializeField] private InventoryManager inventoryManager;

    private Camera _playerCamera;
    private CharacterController characterController;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    public static PlayerController localPlayerController;

    // used when spawning the networked object
    protected override void OnSpawned()
    {
        base.OnSpawned();
        enabled = isOwner;
        if (!isOwner ) { return; }

        localPlayerController = this;
        _playerCamera = Camera.main;
        characterController = GetComponent<CharacterController>();

        SetShadowsOnly(); // removes player model from POV

        _playerCamera.transform.SetParent(transform);
        _playerCamera.transform.localPosition = cameraOffset;

        itemPoint.SetParent(rightHand.transform);

        // returns if player camera is not assigned AKA not owner, or error
        if (_playerCamera == null )
        {
            enabled = false;
            return;
        }
    }

    // removes your player model from your POV and leaves shadow
    private void SetShadowsOnly()
    {
        foreach (var renderer in playerRenderers)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
    }

    // used for despawning the networked object
    protected override void OnDespawned()
    {
        base.OnDespawned();
        if (!isOwner ) { return; }

        localPlayerController = null;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        bool isGrounded = IsGrounded();
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        // Set animator parameters to enable animations for movement
        playerAnimator.SetFloat("forward", vertical);
        playerAnimator.SetFloat("right", horizontal);
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        _playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.03f, Vector3.down, groundCheckDistance);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.03f, Vector3.down * groundCheckDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.TransformPoint(cameraOffset), 0.1f);
    }
#endif
}
