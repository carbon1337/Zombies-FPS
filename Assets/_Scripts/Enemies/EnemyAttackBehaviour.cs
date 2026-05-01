using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackBehaviour : StateMachineBehaviour
{
    EnemyManager EM;

    public float attackDelay;
    bool hasDamaged = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EM = animator.GetComponent<EnemyManager>();

        EM.PlayAttackAudio();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        attackDelay -= Time.deltaTime;

        if(hasDamaged)
        {
            return;
        }

        if(EM.PlayerInRange() && attackDelay <= 0)
        {
            Debug.Log("attacking player");
            //attack the player

            PlayerHealthManager.Instance.TakeDamage(EM.attackDamage);

            hasDamaged = true;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }
}
