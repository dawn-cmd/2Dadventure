using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class PhysicsCheck : MonoBehaviour
{
    private CapsuleCollider2D coll;
    [Header("检测参数")]
    public bool manual;
    private PlayerController playerController;
    public Vector2 bottomOffset;
    public Vector2 leftOffset;
    public Vector2 rightOffset;
    public float checkRadius;
    public LayerMask groundLayer;
    [Header("State")]
    public bool isGround;
    public bool touchLeftWall;
    public bool touchRightWall;
    private void Update()
    {
        Check();
    }
    private void Awake()
    {
        try
        {
            playerController = GetComponent<PlayerController>();
        }
        catch (Exception e) { };
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
        touchLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(leftOffset.x * transform.localScale.x, leftOffset.y), checkRadius, groundLayer);
        touchRightWall = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(rightOffset.x * transform.localScale.x, rightOffset.y), checkRadius, groundLayer);
        // Check Ground
        isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(bottomOffset.x * transform.localScale.x, bottomOffset.y), checkRadius, groundLayer);
        try
        {
            if (isGround == true) playerController.enableDoubleJump = true;
        }
        catch (Exception e) { };
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset, checkRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + leftOffset, checkRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + rightOffset, checkRadius);
    }
}
