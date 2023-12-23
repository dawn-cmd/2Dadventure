using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoarPatrolState : BaseState
{
    public override void OnEnter(Enemy enemy)
    {
        currentEnemy = enemy;
    }
    public override void LogicUpdate()
    {
        // Find enemy change to chase
        if (currentEnemy.physicsCheck.touchLeftWall || !currentEnemy.physicsCheck.isGround)
        {
            currentEnemy.wait = true;
            currentEnemy.anim.SetBool("Walk", false);
        }
        else
        {
            currentEnemy.anim.SetBool("Walk", true);
        }
    }
    public override void PhysicsUpdate()
    {
    }
    public override void OnExit()
    {
        currentEnemy.anim.SetBool("Walk", false);
    }
}
