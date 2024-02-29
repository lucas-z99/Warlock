using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AIState : MonoBehaviour
{

     // base class for all AIState
     // eg. Attack, AlamrAlly, RunAway


     public virtual bool IsValid()
     {
          // check if we should enter this state
          // eg. have I spotted any target?
          // eg. is my nuke-spell ready?

          return true;
     }

     public virtual void OnEnter()
     {
          // eg. choose a target here
     }

     public virtual void UpdateState()
     {
          // eg. looking for target
     }

     public virtual void OnExit()
     {
          //eg. stop a channeling spell
     }



     //protected
     protected AIBrain brain { get { if (_brain == null) _brain = GetComponent<AIBrain>(); return _brain; } }
     AIBrain _brain;


}
