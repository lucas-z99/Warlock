using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public class TEST_NavMeshAgent : MonoBehaviour
{

     public Vector3 velAgent__;
     public bool gizmos;

     [Header("Chase Player")]
     public bool chasePlayer;

     [Header("Ghost Agent")]
     public NavMeshAgent ghost;
     public bool chasePlayer_ghost_agent;
     public bool pause_move_update;
     public float snap_dist = 0.5f;


     [Header("Warp")]
     public Transform warpPosition;
     public bool click_warp;
     public Transform nextPosition;
     public bool click_nextPos;

     [Header("STOP")]
     public bool SetVel0;
     public bool StopNow;
     public bool BREAK;

     [Header("Vel")]
     public Vector3 velocity;
     public bool use_velocity;

     [Header("???")]
     public Vector3 destination;
     public Vector3 agentPos;
     public Vector3 nextPos;
     public bool click_resetPath;
     public float remainingDistance;
     public bool hasPath;
     public bool pathPending;
     public bool isPathStale;
     public NavMeshPathStatus pathStatus;
     public bool isStopped;
     public Vector3 rb_velocity;

     //private
     NavMeshAgent agent;
     Rigidbody rb;
     HPComponent player;


     void Awake()
     {
          agent = GetComponent<NavMeshAgent>();
          rb = GetComponent<Rigidbody>();

          ghost.transform.parent = null; //detach ghost
     }


     void FixedUpdate()
     {
          //if (Time.time < tAddForce + forceMinDuration)
          //     Use_agent_and_kinematic(false);
          //else if (rb.velocity != Vector3.zero)
          //     Use_agent_and_kinematic(false);
          //else
          //     Use_agent_and_kinematic(true);

          rb_velocity = rb.velocity;

          if (chasePlayer_ghost_agent)
          {
               Ghost_agent();
          }

     }


     void Update()
     {
          if (!agent || agent.enabled == false)
               return;

          velAgent__ = agent.velocity;


          //stop
          if (SetVel0) //stop without inertia
          {
               SetVel0 = false;
               agent.velocity = Vector3.zero;
               return;
          }

          if (StopNow) //stop without inertia (0 distance teleport)
          {
               agent.destination = transform.position;
               return;
          }

          agent.isStopped = BREAK; //stop with inertia
          if (BREAK)
               return;

          //velocity
          if (use_velocity)
          {
               agent.velocity = velocity;
          }

          //go to places
          if (chasePlayer)
          {
               player = Getplayer();
               agent.destination = player.transform.position;
          }
          else if (click_nextPos) //teleport, to closest pos if destination is not connected to NavMesh
          {
               click_nextPos = false;
               agent.nextPosition = nextPosition.position; //note teleport will push away/up physical object
          }
          else if (click_warp) //teleport
          {
               click_warp = false;
               agent.Warp(warpPosition.position);
          }

          destination = agent.destination;
          agentPos = agent.transform.position;
          nextPos = agent.nextPosition;
          remainingDistance = agent.remainingDistance;
          hasPath = agent.hasPath;
          pathPending = agent.pathPending;
          isPathStale = agent.isPathStale;
          pathStatus = agent.pathStatus;
          isStopped = agent.isStopped;


          if (click_resetPath)
          {
               click_resetPath = false;
               agent.ResetPath();
          }

     }

     HPComponent Getplayer()
     {
          foreach (var thing in FindObjectsOfType<HPComponent>())
               if (thing.team == CharaTeam.player_main_chara)
                    return thing;

          return null;
     }


     // mix with physics  -------------------------------------------------------

     //float tAddForce;
     //float forceMinDuration = 0.1f;

     //public void OnAddForce(Vector3 _force)
     //{
     //     tAddForce = Time.time;
     //     Use_agent_and_kinematic(false);
     //}

     //void Use_agent_and_kinematic(bool b)
     //{
     //     rb.isKinematic = b;
     //     agent.enabled = b;
     //}

     public float ghost_dist;
     public float ignore_ghost_dist = 0.1f;
     public float ghost_rotate_slerp = 4f;

     void Ghost_agent()
     {
          //target pos
          player = Getplayer();
          ghost.destination = player.transform.position;

          //catch up
          ghost_dist = Vector3.Distance(transform.position, ghost.transform.position);
          var dir = ghost.transform.position - transform.position;

          if (ghost_dist > snap_dist)
          {
               //ghost.transform.position =   transform.position; //and recalculate path
               ghost.transform.position = transform.position + dir.normalized * snap_dist; //and recalculate path
          }

          //pos
          if (!pause_move_update)
               if (ghost_dist > ignore_ghost_dist)
               {
                    transform.position = Vector3.MoveTowards(transform.position, ghost.nextPosition, ghost.speed * Time.fixedDeltaTime);
               }


          //rot
          var to_player = Quaternion.LookRotation(player.transform.position - transform.position);
          var to_corner = Quaternion.LookRotation(ghost.steeringTarget - transform.position);
          to_player = Quaternion.Lerp(to_player, to_corner, 0.5f); //average

          transform.rotation = Quaternion.Slerp(transform.rotation, to_player, ghost_rotate_slerp * Time.fixedDeltaTime);

     }


     void OnDrawGizmos()
     {
          if (!Application.isPlaying)
               return;

          if (!gizmos)
               return;


          Gizmos.color = Color.cyan;
          Gizmos.DrawWireSphere(ghost.transform.position, 0.3f);


          Gizmos.color = Color.red;
          Gizmos.DrawWireSphere(ghost.destination, 0.23f);
          for (int i = 0; i < ghost.path.corners.Length; i++)
          {
               Gizmos.color = Color.green;
               Gizmos.DrawWireSphere(ghost.path.corners[i], 0.25f);
               if (i > 0)
               {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(ghost.path.corners[i], ghost.path.corners[i - 1]);
               }
          }

     }










}
