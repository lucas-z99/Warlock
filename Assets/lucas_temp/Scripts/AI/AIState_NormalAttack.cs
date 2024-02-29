using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UnityEngine;


public class AIState_NormalAttack : AIState
{
     // debug
     public string __monitor;
     public bool __log;
     public bool __gizmos;

     [Header("Setting")]
     public float switchTarget = 5;

     // private
     AITargetData target;
     float tSwitchTarget;


     public override bool IsValid()
     {
          return brain.targets.Count > 0;
     }

     public override void OnEnter()
     {
          //
     }

     public override void UpdateState()
     {
          if (target == null || Time.time > tSwitchTarget) // pick a target
               DecideTarget();

          AttackOrChase();
     }

     public override void OnExit()
     {
          target = null;
          //brain.update_pos = false;
          //brain.update_rot = false;

          if (__log) Debug.Log("OnExitState() = AIState_NormalAttack");
          __monitor = "";
     }


     // decide target  ---------------------------------------------------------------------

     void DecideTarget()
     {
          target = brain.Get_target();
          tSwitchTarget = Time.time + switchTarget;

          if (__log) if (target != null) Debug.Log("DecideTarget() = " + target.hp.name);
     }


     // attack  ---------------------------------------------------------------------
     [Header("TEST Attack")] //TODO: replace with melee spell or something later
     public int attackDamage = 1;
     public float attackRange = 5;
     public float attackCoolDown = 2.8f;
     public float hitDelay = 0.5f; //delay before hit
     public float actionRecover = 1.5f; //time before you can start moving again after an attack
     public bool move_while_attacking;

     public float tLastAttack { get; private set; }
     float tHit;
     float tRecover;
     float tNextAtk;
     bool pending_hit;

     void AttackOrChase()
     {
          // order of event:

          // if hit < recover: AI will stop during and after the attack
          //    (    rotate    )                (        move/rotate
          //    |--------------[!]--------------|---------------|-------
          // initial           hit           recover       allow nextAtk 

          // if hit > recover: AI will only stop during pre-hit
          //    (            rotate            ) (       move/rotate
          //    |---------------|--------------[!]--------------|-------
          // initial         recover           hit         allow nextAtk 

          // if hit > recover, and allow_move_while_attack, AI will be chasing you non-stop

          if (Time.time < tHit) // swinging the sword?
          {
               __monitor = "swing";

               Set_move_target();

               brain.update_pos = move_while_attacking;
               brain.update_rot = true;
          }
          else
          {
               if (pending_hit) // sword hit?
               {
                    if (__log) Debug.Log("TEST_Attack() = " + target.hp.name);

                    pending_hit = false;
                    if (brain.hp.hp > 0) // am I still alive?
                         TEST_Attack(target.hp);
               }

               if (Time.time < tRecover) // recovering from action?
               {
                    __monitor = "recover";
                    brain.update_pos = false;
                    brain.update_rot = false;
               }
               else
               {
                    //initial attack?
                    var dist = Vector3.Distance(transform.position, target.transform.position);
                    if (dist < attackRange)
                    {
                         if (Time.time > tNextAtk)
                         {
                              tLastAttack = Time.time;
                              tHit = Time.time + hitDelay;
                              tRecover = Time.time + actionRecover;
                              tNextAtk = Time.time + attackCoolDown;

                              pending_hit = true;

                              return; //good
                         }
                    }

                    // chase
                    __monitor = "chasing";

                    Set_move_target();
                    brain.update_rot = true;
                    brain.update_pos = true;
               }
          }
     }

     void TEST_Attack(HPComponent hp)
     {
          hp.Damage(attackDamage);
     }

     void Set_move_target()
     {
          brain.Set_move_target(target.transform.position, attackRange * 0.98f); //a bit error space
     }






     // debug ---------------------------------------------------------------------

     void OnDrawGizmosSelected()
     {
          if (!__gizmos)
               return;

          Gizmos.color = Color.red;
          Gizmos.DrawWireSphere(transform.position, attackRange);
     }


}
