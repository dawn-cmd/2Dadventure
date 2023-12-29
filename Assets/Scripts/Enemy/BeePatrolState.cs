using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BeePatrolState : BaseState
{
    Vector3 targetPoint;
    Vector3 moveDir;
    public override void OnEnter(Enemy enemy)
    {
        currentEnemy = enemy;
        currentEnemy.currentSpeed = currentEnemy.normalSpeed;
        targetPoint = currentEnemy.GetNewPoint();
    }
    public override void LogicUpdate()
    {
        if (currentEnemy.FoundPlayer())
            currentEnemy.switchState(NPCstate.Chase);
        if (Math.Abs(targetPoint.x - currentEnemy.transform.position.x) < 0.1f
            && Math.Abs(targetPoint.y - currentEnemy.transform.position.y) < 0.1f)
        {
            currentEnemy.wait = true;
            targetPoint = currentEnemy.GetNewPoint();
        }
        moveDir = (targetPoint - currentEnemy.transform.position).normalized;
        if (moveDir.x > 0)
            currentEnemy.transform.localScale = new Vector3(-1, 1, 1);
        if (moveDir.x < 0)
            currentEnemy.transform.localScale = new Vector3(1, 1, 1);

    }
    public override void PhysicsUpdate()
    {
        if (!currentEnemy.wait && !currentEnemy.isHurt && !currentEnemy.isDead)
        {
            currentEnemy.rb.velocity = moveDir * currentEnemy.currentSpeed * Time.deltaTime;
        }
        else
        {
            currentEnemy.rb.velocity = Vector2.zero;
        }

    }
    public override void OnExit()
    {
        // throw new System.NotImplementedException();
    }
}
