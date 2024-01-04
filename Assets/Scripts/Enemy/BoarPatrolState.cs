using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoarPatrolState : BaseState
{
    public override void OnEnter(Enemy enemy)
    {
        currentEnemy = enemy;
        currentEnemy.currentSpeed = currentEnemy.normalSpeed;
    }
    public override void LogicUpdate()
    {
        // Find enemy change to chase
        if (currentEnemy.FoundPlayer())
        {
            currentEnemy.switchState(NPCstate.Chase);
        }
       if ((currentEnemy.physicsCheck.touchLeftWall && currentEnemy.transform.localScale.x > 0) || (currentEnemy.physicsCheck.touchRightWall && currentEnemy.transform.localScale.x < 0) || !currentEnemy.physicsCheck.isGround) 
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
        Debug.Log("Exit");
    }
}
