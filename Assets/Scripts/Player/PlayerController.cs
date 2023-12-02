using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public PlayerInputControl inputControl;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    public Vector2 inputDirection;
    [Header("Basic Parameter")]
    public float speed;
    public float jumpForce;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        inputControl = new PlayerInputControl();
        inputControl.Gameplay.Jump.started += Jump;

        spriteRenderer = GetComponent<SpriteRenderer>();

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
        // Debug.Log("JUMP");
        rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
    }
}

