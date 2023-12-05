using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private void Awake() {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update() {
        SetAnimation();
    }
    public void SetAnimation() {
        anim.SetFloat("velocityX", rb.velocity.x > 0 ? rb.velocity.x : -rb.velocity.x);
    }
}
