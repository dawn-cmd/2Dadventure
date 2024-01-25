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
    [Header("事件监听")]
    public SceneLoadEventSO loadEventSO;
    public VoidEventSO afterSceneLoadedEvent;

    public PlayerInputControl inputControl;
    public Character character;
    private Rigidbody2D rb;
    private PhysicsCheck physicsCheck;
    private SpriteRenderer spriteRenderer;
    public Vector2 inputDirection;
    private CapsuleCollider2D coll;
    private Vector2 originalOffset;
    private Vector2 originalSize;
    private PlayerAnimation playerAnimation;
    [Header("Basic Parameter")]
    public float speed;
    private float walkSpeed => speed / 2.5f;
    private float runSpeed;
    public float jumpForce;
    public bool enableDoubleJump;
    public int combo;

    [Header("Physical Material")]
    public PhysicsMaterial2D normal;
    public PhysicsMaterial2D wall;

    [Header("States")]
    public bool isCrouch;
    public float HurtForce;
    public float slideDis;
    public float slideSpeed;
    [Header("Power Cost")]
    public int slidePowerCost;
    public int attackPowerCost;
    public int jumpPowerCost;
    public bool isHurt;
    public bool isDead;
    public bool isAttack;
    public bool isSliding;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        character = GetComponent<Character>();
        playerAnimation = GetComponent<PlayerAnimation>();
        inputDirection = Vector2.zero;
        inputControl = new PlayerInputControl();

        // Set of Jump
        inputControl.Gameplay.Jump.started += Jump;

        // Set of Attack
        inputControl.Gameplay.Attack.started += PlayerAttack;
        inputControl.Gameplay.Slide.started += Slide;

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

    private void Slide(InputAction.CallbackContext obj)
    {
        if (!isSliding && physicsCheck.isGround && character.currentPower >= slidePowerCost)
        {
            isSliding = true;
            var targetPos = new Vector3(transform.position.x + slideDis * transform.localScale.x, transform.position.y);
            gameObject.layer = LayerMask.NameToLayer("Enemy");
            StartCoroutine(TriggerSlide(targetPos));
            GetComponent<Character>().OnConsumePower(slidePowerCost);
        }
    }

    private IEnumerator TriggerSlide(Vector3 target)
    {
        do
        {
            yield return null;
            if (!physicsCheck.isGround) break;
            if ((physicsCheck.touchLeftWall && transform.localScale.x < 0) || (physicsCheck.touchRightWall && transform.localScale.x > 0))
            {
                isSliding = false;
                break;
            }
            rb.MovePosition(new Vector2(transform.position.x + slideSpeed * transform.localScale.x, transform.position.y));
        } while (Mathf.Abs(transform.position.x - target.x) > 0.1f);
        isSliding = false;
        gameObject.layer = LayerMask.NameToLayer("Player");
    }
    private void OnEnable()
    {
        inputControl.Enable();
        loadEventSO.LoadRequestEvent += OnLoadRequestEvent;
        afterSceneLoadedEvent.OnEventRaised += OnAfterSceneLoadedEvent;
    }

    

    private void OnDisable()
    {
        inputControl.Disable();
        loadEventSO.LoadRequestEvent -= OnLoadRequestEvent;
        afterSceneLoadedEvent.OnEventRaised -= OnAfterSceneLoadedEvent;
    }
    private void OnLoadRequestEvent(GameSceneSO arg0, Vector3 arg1, bool arg2)
    {
        inputControl.Gameplay.Disable();
    }
    private void OnAfterSceneLoadedEvent()
    {
        inputControl.Gameplay.Enable();
    }
    private void Update()
    {
        inputDirection = inputControl.Gameplay.Move.ReadValue<Vector2>();
        CheckState();
    }

    private void FixedUpdate()
    {
        if (!isHurt && !isAttack) Move();
    }

    // private void OnTriggerStay2D(Collider2D other)
    // {
    //     Debug.Log(other.name);
    // }



    public void Move()
    {
        rb.velocity = new Vector2(isCrouch ? 0 : inputDirection.x * speed * Time.deltaTime, rb.velocity.y);

        // Flip character
        int faceDir = (int)transform.localScale.x;
        if (inputDirection.x > 0) faceDir = 1;
        if (inputDirection.x < 0) faceDir = -1;
        transform.localScale = new Vector3(faceDir, 1, 1);

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
        if (character.currentPower < jumpPowerCost) return;
        // Debug.Log("JUMP");
        if (physicsCheck.isGround == false) enableDoubleJump = false;
        rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        GetComponent<AudioDefinition>().PlayAudioClip();
        character.OnConsumePower(jumpPowerCost);
    }
    #region UnityEvent
    public void GetHurt(Transform attacker)
    {
        isHurt = true;
        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2((transform.position.x - attacker.position.x), 0).normalized;
        rb.AddForce(dir * HurtForce, ForceMode2D.Impulse);
    }

    public void PlayerDead()
    {
        isDead = true;
        inputControl.Gameplay.Disable();
    }
    #endregion

    private void PlayerAttack(InputAction.CallbackContext context)
    {
        if (character.currentPower < attackPowerCost) return;
        playerAnimation.PlayerAttack();
        character.OnConsumePower(attackPowerCost);
        isAttack = true;
    }
    private void CheckState()
    {
        coll.sharedMaterial = physicsCheck.isGround ? normal : wall;
        if (physicsCheck.onWall)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.6f);
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
        }
        if (isDead || isSliding)
        {
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Player");
        }
    }
}

