using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected Rigidbody2D rb;
    [HideInInspector] public Animator anim;
    [HideInInspector] public PhysicsCheck physicsCheck;
    [Header("Basic Parameter")]
    public float normalSpeed;
    public float chaseSpeed;
    [HideInInspector] public float currentSpeed;
    [HideInInspector] public Vector3 faceDir;
    public float hurtForce;
    public Transform attacker;
    [Header("CountDown")]
    public float waitTime;
    public float waitTimeCounter;
    public bool wait;
    [Header("Status")]
    public bool isHurt;
    public bool isDead;
    protected BaseState currentState;
    protected BaseState patrolState;
    protected BaseState chaseState;
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        physicsCheck = GetComponent<PhysicsCheck>();
        currentSpeed = normalSpeed;
        waitTimeCounter = waitTime;
    }
    private void OnEnable()
    {
        currentState = patrolState;
        currentState.OnEnter(this);
    }
    private void Update()
    {
        faceDir = new Vector3(-transform.localScale.x, 0, 0);
        currentState.LogicUpdate();
        TimeCounter();
    }
    private void FixedUpdate()
    {
        if (!isHurt && !isDead && !wait) Move();
        currentState.PhysicsUpdate();
    }
    private void OnDisable()
    {
        currentState.OnExit();
    }
    public virtual void Move()
    {
        rb.velocity = new Vector2(currentSpeed * faceDir.x * Time.deltaTime, rb.velocity.y);
    }
    public void TimeCounter()  // Time Counter
    {
        if (wait)
        {
            waitTimeCounter -= Time.deltaTime;
            if (waitTimeCounter <= 0)
            {
                wait = false;
                waitTimeCounter = waitTime;
                transform.localScale = new Vector3(faceDir.x, 1, 1);
            }
        }
    }

    public void OnTakeDamage(Transform attackTrans)
    {
        attacker = attackTrans;
        // Turn back
        if (attackTrans.position.x - transform.position.x > 0) transform.localScale = new Vector3(-1, 1, 1);
        if (attackTrans.position.x - transform.position.x < 0) transform.localScale = new Vector3(1, 1, 1);
        // Hit Back
        isHurt = true;
        anim.SetTrigger("Hurt");
        Vector2 dir = new Vector2(transform.position.x - attackTrans.position.x, 0).normalized;
        StartCoroutine(OnHurt(dir));
    }
    private IEnumerator OnHurt(Vector2 dir)
    {
        rb.AddForce(dir * hurtForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.45f);
        isHurt = false;
    }
    public void OnDie()
    {
        gameObject.layer = 2;
        anim.SetBool("Dead", true);
        isDead = true;
    }
    public void DestroyAfterAnimation()
    {
        Destroy(this.gameObject);
    }
}
