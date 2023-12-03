using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCheck : MonoBehaviour
{
    private PlayerController playerController;
    public Vector2 bottomOffset;
    public float checkRadius;
    public LayerMask groundLayer;
    [Header("State")]
    public bool isGround;
    private void Update() {
        Check();
    }
    private void Awake() {
        playerController = GetComponent<PlayerController>();
    }
    public void Check()
    {
        // Check Ground
        isGround = Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset, checkRadius, groundLayer);
        if (isGround == true) playerController.enableDoubleJump = true;
    }
    private void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset, checkRadius);
    }
}
