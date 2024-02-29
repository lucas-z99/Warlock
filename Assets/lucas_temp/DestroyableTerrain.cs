using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;


[RequireComponent(typeof(HPComponent))]
public class DestroyableTerrain : NetworkBehaviour
{
     // can be floor or walls
     // it has HP, and you need to define how it behave on destroy

     public bool fall_on_destroy;
     public int destroy_delay = 3000;
     public ParticleSystem destroy_VFX;


     void Awake()
     {
          var hp = GetComponent<HPComponent>();
          hp.On_death_blow += DestroyFloor_ServerRPC;
     }

     void Update()
     {
          if (fall_on_destroy && flag_destroy)
               UpdateFall();
     }

     // end of use
     bool flag_destroy;
     [ServerRpc(RequireOwnership = false)]
     void DestroyFloor_ServerRPC()
     {
          DestroyFloor_ClientRPC();
     }
     [ClientRpc]
     void DestroyFloor_ClientRPC()
     {
          flag_destroy = true;
          Destroy(this, destroy_delay);
     }

     // as floor
     float _vel;
     static float G = 4.5f;

     void UpdateFall()
     {
          _vel -= G * Time.deltaTime;
          transform.position += new Vector3(0, _vel, 0);
     }


     // as wall / cover
     //WIP



}
