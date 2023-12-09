using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the movement and behavior of the player character.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public PlayerInputControl inputControl;
    private Rigidbody2D rb;
    private PhysicsCheck physicsCheck;
    private SpriteRenderer spriteRenderer;
    public Vector2 inputDirection;
    private CapsuleCollider2D coll;
    private Vector2 originalOffset;
    private Vector2 originalSize;
    [Header("Basic Parameter")]
    public float speed;
    private float walkSpeed => speed / 2.5f;
    private float runSpeed;
    public float jumpForce;
    public bool enableDoubleJump;
    public bool isCrouch;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();


        inputControl = new PlayerInputControl();
        inputControl.Gameplay.Jump.started += Jump;


        spriteRenderer = GetComponent<SpriteRenderer>();

        physicsCheck = GetComponent<PhysicsCheck>();
        coll = GetComponent<CapsuleCollider2D>();
        originalOffset = coll.offset;
        originalSize = coll.size;
        #region ForceWalk
        runSpeed = speed;
        inputControl.Gameplay.WalkButton.performed += ctx =>
        {
            if (physicsCheck.isGround) speed = walkSpeed;
        };
        inputControl.Gameplay.WalkButton.canceled += ctx =>
        {
            if (physicsCheck.isGround) speed = runSpeed;
        };
        #endregion  

    }


    private void OnEnable()
    {
        inputControl.Enable();
    }
    private void OnDisable()
    {
        inputControl.Disable();
    }
    private void Update()
    {
        inputDirection = inputControl.Gameplay.Move.ReadValue<Vector2>();

    }

    private void FixedUpdate()
    {
        Move();
    }

    public void Move()
    { 
        rb.velocity = new Vector2(isCrouch ? 0 : inputDirection.x * speed * Time.deltaTime, rb.velocity.y);

        // Flip character
        spriteRenderer.flipX = inputDirection.x <= 0 && (inputDirection.x < 0 || spriteRenderer.flipX);

        // Crouch
        isCrouch = inputDirection.y < -0.5f && physicsCheck.isGround;
        if (isCrouch == true)
        {
            // change block
            coll.offset = new Vector2(-0.05f, 0.85f);
            coll.size = new Vector2(0.66f, 1.6f);
        }
        else
        {
            // reset block
            coll.offset = originalOffset;
            coll.size = originalSize;
        }
    }
    private void Jump(InputAction.CallbackContext context)
    {
        if (physicsCheck.isGround == false && enableDoubleJump == false) return;
        // Debug.Log("JUMP");
        if (physicsCheck.isGround == false) enableDoubleJump = false;
        rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);

    }
}

