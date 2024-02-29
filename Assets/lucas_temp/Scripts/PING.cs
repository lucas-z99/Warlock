using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;

public class PING : NetworkBehaviour
{

     public float pingInterval = 0.5f;
     public bool click_to_reset;
     [Header("RTT")]
     public float RTT;
     public float RTTmax;
     ulong clientID { get => NetworkManager.Singleton.LocalClientId; }



     //private
     List<int> _remove = new List<int>();

     public override void OnNetworkSpawn()
     {
          //clientRPCParams = new ClientRpcParams() { Send = new ClientRpcSendParams() { TargetClientIds = new[] { OwnerClientId } } };
          //clientRPCParams = new ClientRpcParams() { Send = new ClientRpcSendParams() { TargetClientIds = new[] { clientID } } };
          //Debug.Log("clientID " + clientID);

     }

     void Update()
     {
          Ping();
          //PacketTest();

          if (click_to_reset)
          {
               click_to_reset = false;
               RTT = 0;
               RTTmax = 0;
               //PacketLoss = 0;
               //PacketLossRate = 0;
          }
     }

     //private  ----------------------------------------------------------------------------
     Dictionary<int, float> pingList = new Dictionary<int, float>();
     public int pingID;
     float tNextPing;
     ClientRpcParams clientRPCParams;

     void Ping()
     {
          if (IsServer)
               return;

          if (Time.realtimeSinceStartup >= tNextPing)
          {
               tNextPing = Time.realtimeSinceStartup + pingInterval;

               pingList.Add(pingID, Time.realtimeSinceStartup);
               Ping_ServerRPC(pingID, clientID);

               pingID++;
          }
     }

     [ServerRpc(RequireOwnership = false)]
     void Ping_ServerRPC(int _pingID, ulong sender)
     {
          Pong_ClientRPC(_pingID, sender);
     }
     [ClientRpc]
     void Pong_ClientRPC(int _pingID, ulong receiver)
     {
          if (IsServer || clientID != receiver)
               return;

          var pingTime = pingList[_pingID];
          pingList.Remove(_pingID);

          float rtt = (Time.realtimeSinceStartup - pingTime) * 1000;
          RTT += (rtt - RTT) / 2; //average RTT

          //all time RTT and max
          if (rtt > RTTmax)
               RTTmax = rtt;
     }


     ////private  ----------------------------------------------------------------------------
     //public float packetInterval = 0.1f;
     //public float PacketLoss;
     //public float PacketLossRate;

     //float tNextPacket;
     //int packetID;
     //int receivedPacket;
     //ulong clientID { get => NetworkManager.Singleton.LocalClientId; }
     //void PacketTest()
     //{
     //     if (IsServer)
     //     {
     //          if (Time.realtimeSinceStartup >= tNextPacket)
     //          {
     //               tNextPacket = Time.realtimeSinceStartup + packetInterval;
     //               ReceivePacket_ClientRPC(packetID);
     //               packetID++;
     //          }
     //     }
     //}
     //[ClientRpc]
     //void ReceivePacket_ClientRPC()
     //{
     //     if (IsServer)
     //          return;

     //     receivedPacket++;
     //     PacketReply_ServerRPC(clientID);
     //}
     //[ServerRpc(RequireOwnership = false)]
     //void PacketReply_ServerRPC(int _packetID)
     //{

     //}





}
