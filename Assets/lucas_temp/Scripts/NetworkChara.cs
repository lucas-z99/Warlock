using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class NetworkChara : NetworkBehaviour
{

     // Anything that moves - player / enemy / flying axe trap
     // - sync pos / rotation
     // - smooth


     // TEMP
     public static NetworkChara myChara; //TODO: charactor of this cliant, for player controller. swap to other way later
     HPComponent hp; //TODO
     public bool gizmos;


     // private
     NetworkVariable<Vector3> netPos = new(writePerm: NetworkVariableWritePermission.Owner);
     NetworkVariable<Quaternion> netRot = new(writePerm: NetworkVariableWritePermission.Owner);
     bool isSpawned;


     // init ---------------------------------------------------------------------------------
     public override void OnNetworkSpawn()
     {
          // is this player's main chara?
          hp = GetComponent<HPComponent>();
          if (IsOwner && hp && hp.team == CharaTeam.player_main_chara)
          {
               Debug.Assert(myChara == null);
               myChara = this;
          }


          isSpawned = true;
     }
     public override void OnNetworkDespawn()
     {
          if (IsOwner && myChara == this)
               myChara = null;

          isSpawned = false;
     }


     void FixedUpdate()
     {
          if (!isSpawned)
               return;

          if (IsOwner)
          {
               //Server controls all NPC, Player control his own PC
               netRot.Value = transform.rotation;
               netPos.Value = transform.position;
          }
          else
          {
               //Every other PC/NPC only update accordingly
               UpdateRot();
               UpdatePos();
          }
     }


     // smooth --------------------------------------------------------------------------------------------
     Vector3 _vel;
     SmoothSetting setting { get => SmoothSetting.inst; }
     Vector3 pos { get => transform.position; set => transform.position = value; }
     float speedCap { get => hp != null ? hp.team == CharaTeam.player_main_chara ? PlayerSetting.inst.maxSpeed : float.MaxValue : float.MaxValue; }


     void UpdateRot()
     {
          transform.rotation = Quaternion.SlerpUnclamped(transform.rotation, netRot.Value, setting.rotLerp * Time.fixedDeltaTime);
     }

     void UpdatePos()
     {
          if (pos == netPos.Value)
               return;

          if (Vector3.Distance(pos, netPos.Value) <= setting.maxDeviation)
          {
               var predict = pos + setting.extrapolation * (netPos.Value - pos); // TODO: linear extrapolation, if this works well consider non-linear one
               pos = Vector3.MoveTowards(pos, predict, setting.flatVelocity * Time.fixedDeltaTime);

               // default
               if (setting.mode == ClientSmoothMode.Default)
                    pos = Vector3.SmoothDamp(pos, predict, ref _vel, setting.smoothTime, float.MaxValue, Time.fixedDeltaTime);

               // speed cap at
               else if (setting.mode == ClientSmoothMode.SpeedCap)
                    pos = Vector3.SmoothDamp(pos, predict, ref _vel, setting.smoothTime, speedCap * setting.speedCapX, Time.fixedDeltaTime);

               // lerp
               else if (setting.mode == ClientSmoothMode.Lerp)
                    pos = Vector3.LerpUnclamped(pos, predict, setting.posLerp * Time.fixedDeltaTime);
          }
          else
          {
               pos = netPos.Value; //no smooth
          }
     }



     // debug --------------------------------------------------------------------------------------------
     void OnDrawGizmos()
     {
          if (!gizmos)
               return;
          if (Vector3.Distance(transform.position, netPos.Value) > 0.1f)
          {
               Gizmos.color = Color.cyan;
               Gizmos.DrawWireCube(netPos.Value + new Vector3(0, 1, 0), new Vector3(1, 2, 1));
          }

     }


}


