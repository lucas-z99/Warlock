using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PackageTEST : NetworkBehaviour
{

     // package and lag TEST ----------------------------------------------------------------------------
     public NetworkVariable<float> a_number_that_changes = new NetworkVariable<float>();
     public int package_count;
     public float avgPackagePerSec;

     float tStart;

     void FixedUpdate()
     {
          if (IsServer)
               a_number_that_changes.Value = Time.fixedTime;

          avgPackagePerSec = package_count / (Time.fixedTime - tStart);
     }
     public override void OnNetworkSpawn()
     {
          tStart = Time.fixedTime;
          a_number_that_changes.OnValueChanged += (was, current) => package_count += 1;
     }
     //public override void OnNetworkDespawn()
     //{

     //}



}
