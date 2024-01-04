using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoarChaseState : BaseState
{
    public override void LogicUpdate()
    {
        if (currentEnemy.lostCounter <= 0) currentEnemy.switchState(NPCstate.Patrol);
        if ((currentEnemy.physicsCheck.touchLeftWall && currentEnemy.transform.localScale.x > 0) || (currentEnemy.physicsCheck.touchRightWall && currentEnemy.transform.localScale.x < 0) || !currentEnemy.physicsCheck.isGround)
        {
            currentEnemy.transform.localScale = new Vector3(currentEnemy.faceDir.x, 1, 1);
        }
    }

    public override void OnEnter(Enemy enemy)
    {
        currentEnemy = enemy;
        // Debug.Log("Chase");
        currentEnemy.currentSpeed = currentEnemy.chaseSpeed;
        currentEnemy.anim.SetBool("Run", true);
    }

    public override void OnExit()
    {
        currentEnemy.anim.SetBool("Run", false);
    }

    public override void PhysicsUpdate()
    {
    }
}
