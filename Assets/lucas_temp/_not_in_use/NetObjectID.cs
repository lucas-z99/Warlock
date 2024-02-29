using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;


public class NetObjectID : NetworkBehaviour
{

     //identify the same object across different devices

     // public
     public int id { get => _id.Value; }

     public static GameObject Find(int netObjectID)
     {
          if (dict.ContainsKey(netObjectID))
               return dict[netObjectID].gameObject;

          return null;
     }


     //private
     NetworkVariable<int> _id = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);
     static Dictionary<int, NetworkBehaviour> dict = new Dictionary<int, NetworkBehaviour>();

     public override void OnNetworkSpawn()
     {
          //owner assign ID, by default this is client/server who spawn the object
          if (IsOwner)
          {
               _id.Value = GetHashCode();
               dict.Add(_id.Value, this);
          }
     }

     public override void OnNetworkDespawn()
     {
          dict.Remove(_id.Value);
     }

}
