using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnailPatrolState : BaseState
{
    public override void OnEnter(Enemy enemy)
    {
        currentEnemy = enemy;
        currentEnemy.currentSpeed = currentEnemy.normalSpeed;
    }
    public override void LogicUpdate()
    {
        if (currentEnemy.FoundPlayer())
        {
            currentEnemy.switchState(NPCstate.Skill);
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
