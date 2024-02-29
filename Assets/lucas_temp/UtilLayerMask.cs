using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LayerMaskUtil
{

     public static int wall_mask { get => LayerMask.GetMask("Default", "TerrainWithHP"); }

     public static int Get_target_mask(CharaTeam team, bool hitFoe, bool hitFriend, bool isSiege)
     {
          int mask = 0; // who will be hit?

          if (team == CharaTeam.player_main_chara)
          {
               if (hitFoe) mask |= LayerMask.GetMask("Enemy");
               if (hitFriend) mask |= LayerMask.GetMask("Player");
          }
          else
          {
               if (hitFoe) mask |= LayerMask.GetMask("Player");
               if (hitFriend) mask |= LayerMask.GetMask("Enemy");
          }

          if (isSiege) mask |= LayerMask.GetMask("TerrainWithHP");

          return mask;
     }


}
