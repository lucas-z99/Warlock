//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.Linq;
//using System.Threading.Tasks;
//using Unity.Netcode;
//using UnityEngine.AI;


//[RequireComponent(typeof(NetworkChara))]
//[RequireComponent(typeof(HPComponent))]
//public class AIBrain : MonoBehaviour
//{

//     // How to use:
//     // 1) attach an AIState component to GameObject
//     // 2) the order of AIState matters, 

//     // static
//     public static int tDespoawn = 5; //sec

//     public bool physicsMode;

//     [Header("Debug")]
//     public bool gizmos;
//     public bool log;
//     public bool enable_offline_AI = true; //switch on AI when not connected


//     [Header("Basic")]
//     public float move_speed = 10f;
//     public float acc = 20f;
//     public float rotate_speed = 120f;
//     public float mass = 1f; //usually 1, larger mass = less vulnerable to force


//     [Header("Setting")]
//     public float spot_range = 20; //how far will AI spot you?
//     public float alert_nearby = 20; // alert nearby ally when spot enemy


//     // public
//     public bool is_idle { get => current == null; }



//     // private
//     List<AIState> states;
//     AIState current;
//     HPComponent hp;


//     void Awake()
//     {
//          states = new List<AIState>();
//          states.AddRange(GetComponents<AIState>()); //find all state

//          hp = GetComponent<HPComponent>();
//          hp.On_damage_or_heal += On_surprise_attack;
//          hp.On_death_blow += Die;

//          TEST.OnConnect += On_connect;
//          TEST.OnDisconnect += On_local_mode;

//          rb = GetComponent<Rigidbody>();

//          agent = GetComponent<NavMeshAgent>();
//          target_pos = agent.destination;

//          On_local_mode();

//          //check
//          Debug.Assert(states.Count > 0);
//     }

//     void Update()
//     {
//          _updated = false; //reset flags
//          _sorted = false;

//          Update_state();
//          Alert_nearby();
//     }

//     [Header("TEST?")]
//     public Vector3 agentVelocity;
//     public Vector3 target_vel__;
//     public Vector3 manual_vel__;

//     public float mag_agentVelocity;
//     public float mag_target_vel;
//     public float mag_manual;

//     public float gizmos_len = 3f;
//     public float gizmos_r = 0.25f;

//     void OnDrawGizmos()
//     {
//          Gizmos.color = Color.yellow;
//          Gizmos.DrawLine(transform.position, transform.position + agentVelocity * gizmos_len);
//          Gizmos.DrawWireSphere(transform.position + agentVelocity * gizmos_len, gizmos_r);


//          Gizmos.color = Color.blue;
//          Gizmos.DrawLine(transform.position, transform.position + target_vel__ * gizmos_len);
//          Gizmos.DrawWireSphere(transform.position + target_vel__ * gizmos_len, gizmos_r + 0.1f);

//          Gizmos.color = Color.cyan;
//          Gizmos.DrawLine(transform.position, transform.position + manual_vel__ * gizmos_len);
//          Gizmos.DrawWireSphere(transform.position + manual_vel__ * gizmos_len, gizmos_r + 0.2f);

//     }


//     Vector2 forceXZ;
//     float forceY;
//     public bool manually_control_velocity;


//     void FixedUpdate()
//     {
//          agentVelocity = agent.velocity;
//          manual_vel__ = new Vector3(forceXZ.x, forceY, forceXZ.y);
//          target_vel__ = (agent.steeringTarget - transform.position).normalized * move_speed;

//          mag_agentVelocity = agentVelocity.magnitude;
//          mag_target_vel = target_vel__.magnitude;
//          mag_manual = manual_vel__.magnitude;


//          if (physicsMode)
//          {
//               rb.useGravity = true;
//               //if (Time.time < t_enable_agent)
//               //     Use_agent_and_kinematic(false);
//               //else
//               if (rb.velocity != Vector3.zero)
//                    Use_agent_and_kinematic(false);
//               else
//               {
//                    Use_agent_and_kinematic(true);
//                    Update_agent();
//               }
//          }
//          else
//          {
//               rb.useGravity = false;


//               if (manually_control_velocity)
//               {
//                    // (1) upon external force, we start control velocity manually, simulate combined velocity, and try to recover our speed

//                    // apply
//                    agent.velocity = new Vector3(forceXZ.x, 0, forceXZ.y);
//                    agent.baseOffset += forceY * Time.fixedDeltaTime;

//                    // target
//                    var target_vel = (agent.steeringTarget - transform.position).normalized * move_speed;

//                    //xz
//                    forceXZ = Vector2.MoveTowards(forceXZ, new Vector2(target_vel.x, target_vel.z), acc * Time.fixedDeltaTime);

//                    //y
//                    if (agent.baseOffset <= 0)
//                    {
//                         //agent seems to always stay ON the mesh, even if we change Y it will snap back to 0
//                         //so we use baseOffset to simulate Y movement, where the real Y is still handled by agent
//                         agent.baseOffset = 0;
//                         forceY = 0;
//                    }
//                    else
//                    {
//                         forceY += Physics.gravity.y * Time.fixedDeltaTime;
//                    }

//                    // have we recover our speed?
//                    if (forceXZ.x == target_vel.x && forceXZ.y == target_vel.z && forceY == 0)
//                    {
//                         manually_control_velocity = false;
//                         forceXZ = Vector2.zero;
//                         forceY = 0;

//                         //we are going to hand over control to Agent
//                         //the only way is to set agent.velocity to 0
//                         //to avoid a sudden slow down we set acc to infinite for a few frames
//                         _acc = acc;
//                         acc = float.MaxValue;
//                         frame_recover_acc = Time.frameCount + 3;
//                    }
//               }
//               else
//               {
//                    //(2) hand back velocity control to Agent
//                    agent.updateRotation = true;
//                    Update_agent();

//                    if (acc == float.MaxValue && Time.frameCount > frame_recover_acc)
//                         acc = _acc;
//               }
//          }
//     }

//     float _acc;
//     int frame_recover_acc;


//     // network  ----------------------------------------------------------------------
//     void On_connect()
//     {
//          enabled = agent.enabled = NetworkManager.Singleton.IsServer;
//     }
//     void On_local_mode()
//     {
//          enabled = agent.enabled = enable_offline_AI;
//     }


//     // state machine ----------------------------------------------------------------------
//     void Update_state()
//     {
//          AIState next = Decide_next_state();

//          if (next == null)
//          {
//               current = null; //dile
//          }
//          else
//          {
//               // change state?
//               if (current != next)
//               {
//                    if (current)
//                         current.OnExit();
//                    current = next;
//                    current.OnEnter();
//               }

//               // update
//               current.UpdateState();
//          }

//     }

//     AIState Decide_next_state()
//     {
//          foreach (var state in states)
//               if (state.IsValid())
//                    return state;

//          return null; // = idle
//     }


//     // target util  ----------------------------------------------------------------------
//     public List<AITargetData> targets { get => Update_target_list(); }
//     List<AITargetData> _targets = new List<AITargetData>();

//     bool _updated;
//     List<AITargetData> Update_target_list()
//     {
//          if (!_updated)
//          {
//               _updated = true;
//               foreach (var chara in HPComponent.all)
//               {
//                    // hostile?
//                    if (!hp.IsEnemy(chara.team))
//                         continue;

//                    // too far?
//                    var dist = Vector3.Distance(transform.position, chara.transform.position);
//                    if (dist > spot_range)
//                         continue;

//                    // good
//                    var found = _targets.Find(x => x.hp == chara);
//                    if (found == null) // new target?
//                    {
//                         found = new AITargetData();
//                         found.hp = chara;
//                         _targets.Add(found);
//                    }

//                    // update dist
//                    found.dist = dist;
//               }
//          }
//          return _targets;
//     }

//     bool _sorted;
//     public AITargetData Get_target(bool true_closest___false_furthest = true)
//     {
//          if (targets.Count == 0)
//               return null;

//          if (!_sorted)
//          {
//               _sorted = true;
//               targets.OrderBy(x => x.dist);
//          }

//          return true_closest___false_furthest ? targets[0] : targets[targets.Count - 1];
//     }


//     // alert  ----------------------------------------------------------------------
//     bool hasAlert;
//     void Alert_nearby()
//     {
//          if (!hasAlert && targets.Count > 0)
//          {
//               hasAlert = true;
//               Debug.Log("Yeeeha! " + gameObject.name);
//          }
//     }

//     void On_surprise_attack(int value, int hpWas, int hpIs, int attackID)
//     {
//          if (is_idle && value < 0)
//          {
//               var attacker = HPComponent.all.Find(x => x.id == attackID);

//               if (attacker == null)
//                    return;

//               if (!_targets.Exists(x => x.hp.id == attackID))
//               {
//                    var data = new AITargetData();
//                    data.hp = attacker;
//                    _targets.Add(data);
//               }
//               else
//               {
//                    Debug.LogError("this should not happen??");
//               }
//          }
//     }




//     // die  ----------------------------------------------------------------------
//     async void Die()
//     {
//          await Task.Delay(tDespoawn * 1000);
//          Despawn();
//     }

//     void Despawn()
//     {
//          Destroy(gameObject);
//     }


//     // move with navMesh  ---------------------------------------------------------------------

//     NavMeshAgent agent;
//     Vector3 target_pos;
//     Transform chase_after;
//     float reach;
//     int move_mode; //1 = Vector3, 2 = Transform

//     public void Move_towards(Vector3 pos, float _reach)
//     {
//          target_pos = pos; //in Update()
//          reach = _reach;

//          move_mode = 1;
//     }
//     public void Move_towards(Transform _target, float _reach)
//     {
//          chase_after = _target; //in Update()
//          reach = _reach;

//          move_mode = 2;
//     }
//     public void Stop_move()
//     {
//          agent.isStopped = true;
//     }

//     void Update_agent()
//     {
//          if (move_mode == 2) //in FixedUpdate()
//               target_pos = chase_after.position;

//          if (transform.position == target_pos)
//          {
//               agent.isStopped = true;
//               move_mode = 0;
//          }
//          else
//          {
//               agent.isStopped = false;
//               agent.destination = target_pos;

//               agent.stoppingDistance = reach;
//               agent.speed = move_speed;
//               agent.angularSpeed = rotate_speed;
//               agent.acceleration = acc;
//          }
//     }


//     // physics with navMesh  ---------------------------------------------------------------------
//     Rigidbody rb;
//     public void Add_force(Vector3 force)
//     {

//          if (physicsMode)
//          {
//               Use_agent_and_kinematic(false);
//               rb.AddForce(force, ForceMode.Impulse);
//          }
//          else
//          {
//               if (!manually_control_velocity)
//               {
//                    manually_control_velocity = true;

//                    forceXZ.x = agent.velocity.x;
//                    forceXZ.y = agent.velocity.z;
//                    forceY = agent.velocity.y;
//               }

//               forceXZ.x += force.x / mass;
//               forceXZ.y += force.z / mass;
//               forceY += force.y / mass;
//          }
//     }

//     void Use_agent_and_kinematic(bool b)
//     {
//          if (physicsMode)
//          {
//               rb.isKinematic = b;
//               agent.enabled = b;

//               //if (b)
//               //     agent.nextPosition = transform.position;
//          }
//     }


//     // debug  ----------------------------------------------------------------------

//     void OnDrawGizmosSelected()
//     {
//          if (!gizmos)
//               return;

//          Gizmos.color = Color.yellow;
//          Gizmos.DrawWireSphere(transform.position, spot_range);
//     }

//}





//// TEST move and rorate ---------------------------------------------------------------------

////public void Move_towards_TEST(Transform target, float range, float speedPct = 1f)
////{
////     if (moveSpeed == 0)
////          return;
////     if (Vector3.Distance(target.position, transform.position) < range)
////          return;

////     transform.position = Vector3.MoveTowards(
////          transform.position,
////          target.position,
////          moveSpeed * speedPct * Time.deltaTime);
////}

////public void Rotate_towards_TEST(Transform target)
////{
////     if (rotateSpeed == 0)
////          return;

////     var dir = target.position - transform.position;
////     dir.y = 0;
////     var rot = Quaternion.LookRotation(dir);
////     transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, rotateSpeed * Time.deltaTime);
////}







