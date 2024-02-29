using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DropManager : MonoBehaviour
{

     // singleton, auto init

     // OnDeath(source) - DONE
     // calculate chance - DONE
     // generate drop


     [Header("Prefab")]
     public GameObject prefab;

     public float spawnOffsetY = 1;
     public float spawnRadius = 0.5f;
     public float velocityXZ = 5;
     public bool clickToTest;

     [Header("TEST")]
     public DropTable test;


     void Update()
     {
          if (clickToTest)
          {
               clickToTest = false;
               Drop(gameObject.transform.position, test);
          }
     }


     //roll ---------------------------------------------------------------
     public void Drop(Vector3 pos, DropTable table)
     {
          var drop = Roll(table);

          if (drop.Length == 0)
               return;

          pos.y += spawnOffsetY;

          EjectItem(drop[0], pos);

          for (int i = 1; i < drop.Length; i++)
          {
               var spawnPos = pos + new Vector3(Random.Range(-spawnRadius, spawnRadius), 0, Random.Range(-spawnRadius, spawnRadius));
               EjectItem(drop[i], spawnPos);
          }
     }

     void EjectItem(GameObject prefab, Vector3 pos)
     {
          var gameobj = Instantiate(prefab);
          gameobj.transform.position = pos;

          var item = gameobj.GetComponent<ItemGameObject>();
          //item.ui_name =
          //     item.ui_description =



     }



     //roll ---------------------------------------------------------------

     //roll the dice, decide which loot(s) will drop
     GameObject[] Roll(DropTable table)
     {
          if (!table.trim)
               table.Trim();

          if (table.Probability == DropMode.EverythingIsPossible)
               return RollFreeForAll(table);

          if (table.Probability == DropMode.OneAmongAll)
               return RollOneAmongAll(table);

          return null;
     }


     List<GameObject> cache = new List<GameObject>();
     GameObject[] RollFreeForAll(DropTable table)
     {
          cache.Clear();

          foreach (var entry in table.entries)
               if (UnityEngine.Random.Range(0, 100) < entry.chance)
                    cache.Add(entry.obj);

          var result = cache.ToArray();

          return result;
     }


     GameObject[] RollOneAmongAll(DropTable table)
     {
          int sum = 0;
          foreach (var drop in table.entries)
               sum += drop.chance;

          var roll = 1 + UnityEngine.Random.Range(0, sum);

          int i = 0;
          for (; i < table.entries.Count; i++)
               if (roll <= table.entries[i].chance)
                    break;
               else
                    roll -= table.entries[i].chance;

          var result = new GameObject[] { table.entries[i].obj };
          return result;
     }




     //// pool ---------------------------------------------------------------------------------

     //ObjectPool<ItemGameObject> pool;

     //void InitPool()
     //{
     //     var size = 200;
     //     var sizeCap = 500;
     //     pool = new ObjectPool<ItemGameObject>(CreateNew, null, null, null, false, size, sizeCap);
     //}

     //ItemGameObject CreateNew()
     //{
     //     var gameObject = Instantiate(prefab, transform);
     //     gameObject.SetActive(false);

     //     var item = gameObject.GetComponent<ItemGameObject>();
     //     item.mgr = this;

     //     return item;
     //}

     //public void Recycle(ItemGameObject item)
     //{
     //     item.enabled = false;
     //     item.gameObject.SetActive(false);

     //     pool.Release(item);
     //}




}

