using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.AI;
using UnityEngine;
using System.Collections;




/// <summary>
/// A prototype component for syncing NavMeshAgents
/// </summary>
[AddComponentMenu("MLAPI/NetworkNavMeshAgent")]
[RequireComponent(typeof(NavMeshAgent))]
public class NetworkNavMeshAgent : NetworkBehaviour
{
     private NavMeshAgent m_Agent;

     public bool EnableProximity = false;// Is proximity enabled
     public float ProximityRange = 50f;// The proximity range
     public float CorrectionDelay = 3f;// The delay in seconds between corrections

     [Tooltip("Everytime a correction packet is received. This is the percentage (between 0 & 1) that we will move towards the goal.")]
     public float DriftCorrectionPercentage = 0.1f;// The percentage to lerp on corrections
     public bool WarpOnDestinationChange = false;// Should we warp on destination change

     private void Awake()
     {
          m_Agent = GetComponent<NavMeshAgent>();
     }

     private Vector3 m_LastDestination = Vector3.zero;
     private float m_LastCorrectionTime = 0f;

     private void Update()
     {
          if (!IsOwner) return;

          if (m_Agent.destination != m_LastDestination)
          {
               m_LastDestination = m_Agent.destination;
               if (!EnableProximity)
               {
                    OnNavMeshStateUpdateClientRpc(m_Agent.destination, m_Agent.velocity, transform.position);
               }
               else
               {
                    var proximityClients = new List<ulong>();
                    foreach (KeyValuePair<ulong, NetworkClient> client in NetworkManager.Singleton.ConnectedClients)
                    {
                         if (client.Value.PlayerObject == null || Vector3.Distance(client.Value.PlayerObject.transform.position, transform.position) <= ProximityRange)
                         {
                              proximityClients.Add(client.Key);
                         }
                    }

                    OnNavMeshStateUpdateClientRpc(m_Agent.destination, m_Agent.velocity, transform.position, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = proximityClients.ToArray() } });
               }
          }

          if (true)
          //if (NetworkManager.Singleton.ServerTime.fl - m_LastCorrectionTime >= CorrectionDelay)
          {
               if (!EnableProximity)
               {
                    OnNavMeshCorrectionUpdateClientRpc(m_Agent.velocity, transform.position);
               }
               else
               {
                    var proximityClients = new List<ulong>();
                    foreach (KeyValuePair<ulong, NetworkClient> client in NetworkManager.Singleton.ConnectedClients)
                    {
                         if (client.Value.PlayerObject == null || Vector3.Distance(client.Value.PlayerObject.transform.position, transform.position) <= ProximityRange)
                         {
                              proximityClients.Add(client.Key);
                         }
                    }

                    OnNavMeshCorrectionUpdateClientRpc(m_Agent.velocity, transform.position, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = proximityClients.ToArray() } });
               }

               //m_LastCorrectionTime = NetworkManager.Singleton.NetworkTime;
          }
     }

     [ClientRpc]
     private void OnNavMeshStateUpdateClientRpc(Vector3 destination, Vector3 velocity, Vector3 position, ClientRpcParams rpcParams = default)
     {
          m_Agent.Warp(WarpOnDestinationChange ? position : Vector3.Lerp(transform.position, position, DriftCorrectionPercentage));
          m_Agent.SetDestination(destination);
          m_Agent.velocity = velocity;
     }

     [ClientRpc]
     private void OnNavMeshCorrectionUpdateClientRpc(Vector3 velocity, Vector3 position, ClientRpcParams rpcParams = default)
     {
          m_Agent.Warp(Vector3.Lerp(transform.position, position, DriftCorrectionPercentage));
          m_Agent.velocity = velocity;
     }
}

