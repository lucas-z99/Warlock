using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class TEST_NetSpawn : NetworkBehaviour
{
     //1 net spawn -
     //2 net despawn - 
     //3 give and return ownership

     public NetworkVariable<float> netFloat = new NetworkVariable<float>();
     //NetworkVariable<float> everyoneCanChange = new(NetworkVariableWritePermission.Server);


     void Awake()
     {
          Debug.Log("Awake");
          to_be_sure_of_time_ServerRpc();
     }
     void Start()
     {
          Debug.Log("Start");
     }

     void Update()
     {
          if (IsServer)
               netFloat.Value = Time.time;

          //netFloat.Value = Time.time;
     }

     public override void OnNetworkSpawn()
     {
          base.OnNetworkSpawn();
          Debug.Log("OnNetworkSpawn");

     }
     public override void OnNetworkDespawn()
     {
          base.OnNetworkDespawn();
          Debug.Log("OnNetworkDespawn");

     }

     [ServerRpc(RequireOwnership = false)]
     public void to_be_sure_of_time_ServerRpc()
     {
          if (IsServer)
               Debug.Log("----------------");
     }

     protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
     {
          base.OnSynchronize(ref serializer);
          Debug.Log("OnSynchronize");
          Debug.Log("typeof = " + typeof(T));
          if (serializer.IsReader)
               Debug.Log(" serializer.IsReader");
          if (serializer.IsWriter)
               Debug.Log(" serializer.IsWriter");

     }



     public override void OnGainedOwnership()
     {
          base.OnGainedOwnership();
          Debug.Log("OnGainedOwnership");

     }
     public override void OnLostOwnership()
     {
          base.OnLostOwnership();
          Debug.Log("OnLostOwnership");

     }



}
