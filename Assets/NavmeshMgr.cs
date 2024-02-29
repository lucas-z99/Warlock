using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;


public class NavmeshMgr : MonoBehaviour
{

     // singleton
     public static NavmeshMgr inst;
     ObjectPool<NavMeshAgent> pool;


     void Awake()
     {
          Debug.Assert(inst == null);
          inst = this;

          InitPool();
     }

     public static NavMeshAgent GetAgent()
     {
          return inst.pool.Get();
     }

     public static void Recycle(NavMeshAgent agent)
     {
          agent.enabled = false;
          agent.gameObject.SetActive(false);

          inst.pool.Release(agent);
     }

     void InitPool()
     {
          var size = 200;
          var sizeCap = 500;
          pool = new ObjectPool<NavMeshAgent>(CreateNew, null, null, null, false, size, sizeCap);
     }

     NavMeshAgent CreateNew()
     {
          var obj = new GameObject();
          obj.transform.parent = inst.transform;
          obj.SetActive(false);

          var agent = obj.AddComponent<NavMeshAgent>();
          agent.enabled = false;

          return agent;
     }





}
