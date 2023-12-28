using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnailSkillState : BaseState
{
    public override void OnEnter(Enemy enemy)
    {
        currentEnemy = enemy;
        currentEnemy.currentSpeed = currentEnemy.chaseSpeed;
        currentEnemy.anim.SetBool("Walk", false);
        currentEnemy.anim.SetBool("Hide", true);
        currentEnemy.anim.SetTrigger("Skill");
        currentEnemy.lostCounter = currentEnemy.lostTime;
        currentEnemy.GetComponent<Character>().invulnerable = true;
        currentEnemy.GetComponent<Character>().SetInvulnerableCounter(currentEnemy.lostCounter);
    }
    public override void LogicUpdate()
    {
        if (currentEnemy.lostCounter <= 0) currentEnemy.switchState(NPCstate.Patrol);
        currentEnemy.GetComponent<Character>().SetInvulnerableCounter(currentEnemy.lostCounter);
    }
    public override void PhysicsUpdate()
    {
    }
    public override void OnExit()
    {
        currentEnemy.anim.SetBool("Hide", false);
        currentEnemy.GetComponent<Character>().invulnerable = false;
    }
}
