using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovementBehaviour : StateMachineBehaviour
{
    EnemyManager EM;

    string nextAttack;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EM = animator.GetComponent<EnemyManager>();

        nextAttack = EM.RandomAttack();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if(EM.target != null)
        {
            EM.MoveTowardsPlayer();
        } else
        {
            animator.SetBool("canMove", false);
        }

        if(EM.IsInAttackRange())
        {
            animator.SetTrigger(nextAttack);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(nextAttack);
    }
}
