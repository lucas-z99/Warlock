using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.AI;
using System.Linq;
using Unity.VisualScripting;
using JetBrains.Annotations;
using UnityEngine.Rendering.PostProcessing;

[RequireComponent(typeof(NetworkChara))]
[RequireComponent(typeof(HPComponent))]
public class AIBrain : MonoBehaviour
{

     // How to use:
     // 1) attach any AIState to the same GameObject, eg. AIState_NormalAttack
     // 2) the order of those state is priority, see Update_state_machine()
     // 3) handle navMesh agent

     // static
     public static int tDespoawn = 5; //sec


     // test
     public string __monitor;
     public bool __gizmos;
     public bool __offline_AI = true; //switch on AI when not connected

     // setting
     [Header("Move")]
     public float move_speed = 10f;
     public float maxAcc = 20f; //agent dont use this acc, it has infinite acc to stay a bit ahead of us
     public float rotate_speed = 120f;
     [Header("Alert")]
     public float spot_range = 20; //how far will AI spot you?
     public float alert_nearby = 20; // alert nearby ally when spot enemy


     // public
     [HideInInspector] public bool update_rot;
     [HideInInspector] public bool update_pos;
     public HPComponent hp { get; private set; }


     // private
     List<AIState> states;
     AIState current;
     Rigidbody rb;
     NavMeshAgent agent;


     void Awake()
     {
          rb = GetComponent<Rigidbody>();

          states = new List<AIState>();
          states.AddRange(GetComponents<AIState>()); //find all state

          hp = GetComponent<HPComponent>();
          hp.On_damage_or_heal += On_surprise_attack;
          hp.On_death_blow += Die;

          var phy_mat = GetComponent<Collider>().material; //disable friction
          phy_mat.dynamicFriction = 0;
          phy_mat.staticFriction = 0;
          phy_mat.frictionCombine = PhysicMaterialCombine.Minimum;

          agent = GetComponentInChildren<NavMeshAgent>();
          agent.transform.parent = null; //detached 'ghost' agent
          agent.acceleration = float.MaxValue;

          TEST.OnConnect += On_connect;
          TEST.OnDisconnect += On_local_mode;

          //update_pos = false;
          //update_rot = false;

          On_local_mode();

          Debug.Assert(states.Count > 0); //check
     }

     void Update()
     {
          _updated = false; //reset flags
          _sorted = false;

          Update_state_machine();
          Alert_nearby();
     }

     void FixedUpdate()
     {
          Update_pos();
          Update_rotation();
     }

     void OnDestroy()
     {
          Destroy(agent);
     }


     // network  ----------------------------------------------------------------------
     void On_connect()
     {
          enabled = agent.enabled = NetworkManager.Singleton.IsServer;
     }
     void On_local_mode()
     {
          enabled = agent.enabled = __offline_AI;
     }


     // state machine ----------------------------------------------------------------------
     void Update_state_machine()
     {
          AIState next = Decide_next_state();

          if (next == null)
          {
               current = null; //dile
          }
          else
          {
               if (current != next) //change state?
               {
                    //clean up
                    if (current)
                         current.OnExit();
                    update_pos = false;
                    update_rot = false;

                    //next
                    current = next;
                    current.OnEnter();
               }

               // update
               current.UpdateState();
          }

          __monitor = current == null ? "Idle" : current.GetType().ToString();
     }

     AIState Decide_next_state()
     {
          foreach (var state in states)
               if (state.IsValid())
                    return state;

          return null; // = idle
     }


     // target list  ----------------------------------------------------------------------
     public List<AITargetData> targets { get => Update_target_list(); }
     List<AITargetData> _targets = new List<AITargetData>();

     bool _updated;
     List<AITargetData> Update_target_list()
     {
          if (!_updated)
          {
               _updated = true;
               foreach (var chara in HPComponent.all)
               {
                    // hostile?
                    if (!hp.IsEnemy(chara.team))
                         continue;

                    // too far?
                    var dist = Vector3.Distance(transform.position, chara.transform.position);
                    if (dist > spot_range)
                         continue;

                    // good
                    var found = _targets.Find(x => x.hp == chara);
                    if (found == null) // new target?
                    {
                         found = new AITargetData();
                         found.hp = chara;
                         _targets.Add(found);
                    }

                    // update dist
                    found.dist = dist;
               }
          }
          return _targets;
     }

     bool _sorted;
     public AITargetData Get_target(bool true_closest___false_furthest = true)
     {
          if (targets.Count == 0)
               return null;

          if (!_sorted)
          {
               _sorted = true;
               targets.OrderBy(x => x.dist);
          }

          return true_closest___false_furthest ? targets[0] : targets[targets.Count - 1];
     }


     // alert  ----------------------------------------------------------------------
     bool hasAlert;
     void Alert_nearby()
     {
          if (!hasAlert && targets.Count > 0)
          {
               hasAlert = true;
               Debug.Log("Yeeeha! " + gameObject.name);
          }
     }

     void On_surprise_attack(int value, int hpWas, int hpIs, int attackID)
     {
          if (current == null && value < 0)
          {
               var attacker = HPComponent.all.Find(x => x.id == attackID);

               if (attacker == null)
                    return;

               if (!_targets.Exists(x => x.hp.id == attackID))
               {
                    var data = new AITargetData();
                    data.hp = attacker;
                    _targets.Add(data);
               }
               else
               {
                    Debug.LogError("this should not happen??");
               }
          }
     }


     // die  ----------------------------------------------------------------------
     async void Die()
     {
          await Task.Delay(tDespoawn * 1000);
          Destroy(gameObject);
     }


     // using navMesh  ---------------------------------------------------------------------

     float agent_pos_devation = 0.5f;
     float agent_rot_devation = 10f; //degree
     float agent_jitter_filter = 0.1f; //agent's NextPos is simulated, and it always jitters a bit
     float agent_rotate_slerp = 4f;

     public void Set_move_target(Vector3 pos, float stopping_distance)
     {
          update_pos = true;

          agent.destination = pos;
          agent.stoppingDistance = stopping_distance;
          agent.speed = move_speed;
          agent.angularSpeed = rotate_speed;
     }

     void Update_pos()
     {
          __rb_vel = rb.velocity;
          __rb_vel_mag = rb.velocity.magnitude;

          if (!update_pos)
          {
               rb.velocity = Vector3.zero;
               return;
          }

          // if agent deviate too far, snap back
          var dist = Vector3.Distance(transform.position, agent.transform.position);
          if (dist > agent_pos_devation)
          {
               agent.transform.position =
                    transform.position
                    + (agent.transform.position - transform.position).normalized * agent_pos_devation;
          }

          // move
          if (dist > agent_jitter_filter)
          {
               var vel_desired = (agent.nextPosition - transform.position).normalized * move_speed;
               var acc = vel_desired - rb.velocity;
               acc.y = 0;
               acc = Vector3.ClampMagnitude(acc, maxAcc);

               rb.AddForce(acc, ForceMode.Acceleration);

               __acc = acc;
               __accMag = acc.magnitude;
               __vel_desired = vel_desired;
               __vel_desired_mag = vel_desired.magnitude;
          }
     }



     void Update_rotation()
     {
          if (!update_rot)
               return;

          // if rotation deviate too far, snap back
          var angle = Quaternion.Angle(transform.rotation, agent.transform.rotation);
          if (angle > agent_rot_devation)
               agent.transform.rotation = transform.rotation;

          // some average
          var to_player = Quaternion.LookRotation(agent.destination - transform.position);
          var to_corner = Quaternion.LookRotation(agent.steeringTarget - transform.position);
          var rot = Quaternion.Lerp(to_player, to_corner, 0.5f);

          // apply
          transform.rotation = Quaternion.Slerp(transform.rotation, rot, agent_rotate_slerp * Time.fixedDeltaTime);
     }




     // debug  ----------------------------------------------------------------------
     [Header("debug")]
     public Vector3 __vel_desired;
     public float __vel_desired_mag;

     public Vector3 __rb_vel;
     public float __rb_vel_mag;

     public Vector3 __acc;
     public float __accMag;



     void OnDrawGizmos()
     {
          if (!Application.isPlaying)
               return;

          if (!__gizmos)
               return;

          // alert range
          Gizmos.color = Color.yellow;
          Gizmos.DrawWireSphere(transform.position, spot_range);


          // nav agent
          Gizmos.color = Color.red;
          Gizmos.DrawLine(transform.position, agent.transform.position);

          Gizmos.color = Color.green;
          for (int i = 0; i < agent.path.corners.Length; i++)
          {
               Gizmos.DrawWireSphere(agent.path.corners[i], 0.25f);
               if (i > 0)
                    Gizmos.DrawLine(agent.path.corners[i], agent.path.corners[i - 1]);
          }


     }







}











