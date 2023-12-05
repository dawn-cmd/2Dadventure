using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public PlayerInputControl inputControl;
    private Rigidbody2D rb;
    private PhysicsCheck physicsCheck;
    private SpriteRenderer spriteRenderer;
    public Vector2 inputDirection;
    [Header("Basic Parameter")]
    public float speed;
    private float walkSpeed => speed / 2.5f;
    private float runSpeed;
    public float jumpForce;
    public bool enableDoubleJump;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();


        inputControl = new PlayerInputControl();
        inputControl.Gameplay.Jump.started += Jump;


        spriteRenderer = GetComponent<SpriteRenderer>();

        physicsCheck = GetComponent<PhysicsCheck>();
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
        rb.velocity = new Vector2(inputDirection.x * speed * Time.deltaTime, rb.velocity.y);

        // Flip character
        spriteRenderer.flipX = inputDirection.x <= 0 && (inputDirection.x < 0 || spriteRenderer.flipX);
    }
    private void Jump(InputAction.CallbackContext context)
    {
        if (physicsCheck.isGround == false && enableDoubleJump == false) return;
        // Debug.Log("JUMP");
        if (physicsCheck.isGround == false) enableDoubleJump = false;
        rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);

    }
}

