using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public enum CharaTeam
{
     player_main_chara = 10,
     enemy = 20,
     terrain = 30, //NPC?
}


public class HPComponent : NetworkBehaviour
{

     // Anything that has HP - player / NPC / breakable wall
     // - handle HP
     // - friend or foe
     // - layer mask for damage collision
     // - network ID (bonus!)


     public string monitor;

     // setting
     public int set_max_hp = 10;
     public CharaTeam team = CharaTeam.enemy;
     public bool requireSiegeAttack; //soft terrain can only be damaged by siege attack

     // static
     public static List<HPComponent> all = new List<HPComponent>(); // for targeting

     // public
     public int maxHP { get; private set; }
     public int hp { get; private set; }
     public Action<int, int, int, int> On_damage_or_heal; // (damage, hp was, hp is, attackerID), eg. you deal 999 damage to X who has 20 HP. Then damage=999, hp was 20, is 0
     public Action<int, int> On_config_hp; // (hp, maxHP), for Init HP or passive +maxHP, or similar but isn't a heal
     public Action On_death_blow;
     public int id { get => _id.Value; }


     // private
     NetworkVariable<int> _id = new NetworkVariable<int>();


     // initial  ----------------------------------------------------------------------------

     public override void OnNetworkSpawn()
     {
          if (IsServer)
               _id.Value = GetHashCode();

          Config_hp(set_max_hp, set_max_hp); //initial hp

          all.Add(this);

          //check
          Debug.Assert(set_max_hp > 0);
          if (team == CharaTeam.player_main_chara) if (gameObject.layer != LayerMask.NameToLayer("Player")) Debug.LogError(gameObject.name);
          if (team == CharaTeam.enemy) if (gameObject.layer != LayerMask.NameToLayer("Enemy")) Debug.LogError(gameObject.name);
          if (team == CharaTeam.terrain) if (gameObject.layer != LayerMask.NameToLayer("TerrainWithHP")) Debug.LogError(gameObject.name);
     }

     public override void OnNetworkDespawn()
     {
          all.Remove(this);
     }


     // config HP  ----------------------------------------------------------------------------

     // for initial HP, or passive skill +maxHP. They are not healing so won't show UI number
     public void Config_hp(int newHP, int newMaxHP)
     {
          var packet = new ConfigPacket();
          packet.HP = newHP;
          packet.maxHP = newMaxHP;

          Config_hp_ServerRPC(packet);
     }

     [ServerRpc(RequireOwnership = false)]
     void Config_hp_ServerRPC(ConfigPacket data)
     {
          Config_hp_ClientRPC(data);
     }
     [ClientRpc]
     void Config_hp_ClientRPC(ConfigPacket data)
     {
          hp = data.HP;
          maxHP = data.maxHP;

          On_config_hp?.Invoke(hp, maxHP); //event
     }

     struct ConfigPacket : INetworkSerializable
     {
          public int HP;
          public int maxHP;
          void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
          {
               serializer.SerializeValue(ref HP);
               serializer.SerializeValue(ref maxHP);
          }
     }


     // damage or heal  ----------------------------------------------------------------------------

     /// <summary> If player is attacking, include attackerID or AI wouldn't know who </summary>
     public void Damage(int value, int attackerID = -1, bool isSiege = false)
     {
          if (requireSiegeAttack && !isSiege)
               return;

          var data = new DamageHealPacket();
          data.delta = -value; //damage is -
          data.attackerID = attackerID;
          Damage_or_heal_ServerRPC(data);

          //check
          Debug.Assert(value > 0);
     }
     public void Heal(int value)
     {
          var data = new DamageHealPacket();
          data.delta = value;
          data.attackerID = -1;
          Damage_or_heal_ServerRPC(data);

          //check
          Debug.Assert(value > 0);
     }

     [ServerRpc(RequireOwnership = false)]
     void Damage_or_heal_ServerRPC(DamageHealPacket data)
     {
          Damage_or_heal_ClientRPC(data);
     }
     [ClientRpc]
     void Damage_or_heal_ClientRPC(DamageHealPacket data)
     {
          var was = hp;
          hp = Mathf.Clamp(hp + data.delta, 0, maxHP);

          monitor = hp + "/" + maxHP; // debug
          On_damage_or_heal?.Invoke(data.delta, was, hp, data.attackerID); // event
          UIDamageTextMgr.DisplayDamageText(data.delta, gameObject, team == CharaTeam.player_main_chara && IsOwner); // ui

          if (was != 0 && hp == 0) // dead?
          {
               if (team == CharaTeam.enemy)
                    TEST_BecomeRagdoll();

               On_death_blow?.Invoke(); // event
          }
     }

     struct DamageHealPacket : INetworkSerializable
     {
          public int delta;
          public int attackerID;
          void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
          {
               serializer.SerializeValue(ref delta);
               serializer.SerializeValue(ref attackerID);
          }
     }


     // friend or foe  ----------------------------------------------------------------------------
     public bool IsEnemy(CharaTeam team)
     {
          return this.team != team;
     }


     // fun for NPC --------------------------------------------------------------------------------------------
     void TEST_BecomeRagdoll()
     {
          var rb = GetComponent<Rigidbody>();
          if (rb)
          {
               rb.constraints = 0; //80 = freezeXZ, 0 = no contraint
               rb.mass /= 3f;
          }

          var fc = GetComponentInChildren<FaceCamera>();
          if (fc)
          {
               fc.End_of_use();
          }
     }



}
