using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeChaseState : BaseState
{
    private Attack attack;
    Vector3 targetPoint;
    Vector3 moveDir;
    private bool is_attacking;
    private float attack_rate_counter = 0;
    public override void OnEnter(Enemy enemy)
    {
        currentEnemy = enemy;
        currentEnemy.currentSpeed = currentEnemy.chaseSpeed;
        attack = currentEnemy.GetComponent<Attack>();
        currentEnemy.lostCounter = currentEnemy.lostTime;
        currentEnemy.anim.SetBool("Chase", true);
    }
    public override void LogicUpdate()
    {
        if (currentEnemy.lostCounter <= 0) currentEnemy.switchState(NPCstate.Patrol);
        targetPoint = new Vector3(currentEnemy.attacker.transform.position.x, currentEnemy.attacker.transform.position.y + 1.2f, 0);
        attack_rate_counter -= Time.deltaTime;
        // judge attack range
        if (Vector3.Distance(currentEnemy.transform.position, targetPoint) < attack.attackRange)
        {
            // attack 
            if (!currentEnemy.isHurt)
                currentEnemy.rb.velocity = Vector2.zero;
            is_attacking = true;

            // attack rate counter

            if (attack_rate_counter <= 0)
            {
                attack_rate_counter = attack.attackRate;
                currentEnemy.anim.SetTrigger("Attack");
            }
        }
        else
        {
            is_attacking = false;
        }
        moveDir = (targetPoint - currentEnemy.transform.position).normalized;
        if (moveDir.x > 0)
            currentEnemy.transform.localScale = new Vector3(-1, 1, 1);
        if (moveDir.x < 0)
            currentEnemy.transform.localScale = new Vector3(1, 1, 1);
    }
    public override void PhysicsUpdate()
    {
        if (!currentEnemy.isHurt && !currentEnemy.isDead && !is_attacking)
        {
            currentEnemy.rb.velocity = moveDir * currentEnemy.currentSpeed * Time.deltaTime;
        }
    }
    public override void OnExit()
    {
        currentEnemy.anim.SetBool("Chase", false);
    }
}
