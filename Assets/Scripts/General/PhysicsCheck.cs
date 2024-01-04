using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.AccessControl;
using UnityEditor.Callbacks;
using UnityEngine;

public class PhysicsCheck : MonoBehaviour
{
    private CapsuleCollider2D coll;
    [Header("检测参数")]
    public bool manual;
    public bool isPlayer;
    private PlayerController playerController;
    private Rigidbody2D rb;
    public Vector2 bottomOffset;
    public Vector2 leftOffset;
    public Vector2 rightOffset;
    public float checkRadius;
    public LayerMask groundLayer;
    [Header("State")]
    public bool isGround;
    public bool touchLeftWall;
    public bool touchRightWall;
    public bool onWall;
    private void Update()
    {
        Check();
    }
    private void Awake()
    {
        if (isPlayer)
        {
            playerController = GetComponent<PlayerController>();
            rb = GetComponent<Rigidbody2D>();
        }
        coll = GetComponent<CapsuleCollider2D>();
        if (!manual)
        {
            rightOffset = new Vector2((coll.bounds.size.x + coll.offset.x) / 2, coll.bounds.size.y / 2);
            leftOffset = new Vector2(-rightOffset.x, rightOffset.y);
        }
    }
    public void Check()
    {
        // Check wall
        touchLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(leftOffset.x, leftOffset.y), checkRadius, groundLayer);
        touchRightWall = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(rightOffset.x, rightOffset.y), checkRadius, groundLayer);
        // Check Ground
        isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(bottomOffset.x, bottomOffset.y + (onWall ? 0.5f : 0f)), checkRadius, groundLayer);
        if (isPlayer)
        {
            if (isGround || onWall) playerController.enableDoubleJump = true;
        }
        // Check on wall
        if (isPlayer) onWall = ((touchLeftWall && playerController.inputDirection.x < 0f) || (touchRightWall && playerController.inputDirection.x > 0f)) && (rb.velocity.y < 0f);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset, checkRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + leftOffset, checkRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + rightOffset, checkRadius);
    }
}
