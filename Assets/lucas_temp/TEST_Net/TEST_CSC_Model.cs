using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Pool;


public class TEST_CSC_Model : NetworkBehaviour
{
     // CSC model

     // Step 1) client -> server
     //         client.PlayVisual()
     // Step 2) server -> all other client.PlayVisual()
     // Step 3) server OnHit() (eg. bullet collide)
     //         server update NetworkValues (eg. hp, mp)
     //         server -> all client.OnHitVisual()

     // Pro? code are easy to write
     // Con? CtoC will 


     // improvement?
     // CtoC?


     ulong clientID { get => NetworkManager.Singleton.LocalClientId; }
     static string time { get => DateTime.Now.ToString("h:mm:ss"); }


     void Update()
     {
          if (Input.GetKeyDown(KeyCode.Mouse0))
               Fire();
     }


     // test basic RPC ----------------------------------------------------------------------------

     [ServerRpc(RequireOwnership = false)]
     public void TEST_ServerRpc()
     {
          /*
           * ServerRpc (remote procedure call) can be only invoked by a client
           * and will always be received and executed on the server/host
           */

          Debug.Log("TEST_ServerRpc() " + time);
     }

     [ClientRpc]
     public void TEST_ClientRpc()
     {
          Debug.Log("TEST_ClientRpc() " + time);
     }


     // fire ------------------------------------------------------------------------

     public void Fire()
     {
          TEST.GUILog("----------------------------------------------");

          TEST.GUILog("Client " + clientID + " - Fire(): (0) Start");

          if (IsClient)
          {
               //TEST.GUILog("Client " + clientID + ": " + "Fire(): (0) Start");

               Fire_VFX(); // 1) play visual effect
               Fire_ServerRPC(clientID); // 2) inform server
          }
     }

     //server code
     [ServerRpc(RequireOwnership = false)]
     void Fire_ServerRPC(ulong originClientID)
     {
          TEST.GUILog("Client " + clientID + " - Fire_ServerRPC(): (1) Arrive at Server");

          Fire_ClientRPC(originClientID); // ask everyone play visual effect (except the sender)

          // 2) wait for OnHit call from bullet, projectile etc....
     }

     [ClientRpc]
     void Fire_ClientRPC(ulong originClientID)
     {
          if (clientID != originClientID)
          {
               Fire_VFX();
          }

          if (IsServer) Invoke("OnHit", 2);// TODO: TEST CHEATING! replace with a real collider
     }

     void Fire_VFX()
     {
          TEST.GUILog("Client " + clientID + " - Fire_VFX(): (2) Visual Effect");
     }

     public void OnHit()
     {
          if (!IsServer)
          {
               // TODO:
               // here allow client show HP loss instantly
               // and later sync with the HP send by server
               // IMPORTANT event like death/loot should only be handled by server
               return;
          }

          TEST.GUILog("Client " + clientID + " - OnHit(): (3) Server register a hit! Broadcast to clients ");

          OnHitVisual_ClientRPC();
          // NetworkVariable will update state (HP, MP) 
          // so this RPC's job is for eg. destroy the projectile, or maybe on hit effect

     }

     [ClientRpc]
     void OnHitVisual_ClientRPC()
     {
          TEST.GUILog("Client " + clientID + " - OnHitVisual_ClientRPC(): A hit! ");

          // TODO:
          // to compensate visually in case object are diverging on the client side:
          //  - a bullet may instantly travels the last bit distance
          //  - or make a very sharp lerp
     }





}

