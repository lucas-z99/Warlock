using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NPCController : MonoBehaviour
{

     public float moveSpeed = 10f;
     public float rotateSpeed = 10f;



     public void TEST_MoveTowards(NetworkChara target, float reach, float speedPct = 1f)
     {
          if (moveSpeed == 0)
               return;
          if (Vector3.Distance(target.transform.position, transform.position) < reach)
               return;

          transform.position = Vector3.MoveTowards(
               transform.position,
               target.transform.position,
               moveSpeed * speedPct * Time.deltaTime);
     }

     public void TEST_RotateTowards(NetworkChara target)
     {
          if (rotateSpeed == 0)
               return;

          var dir = target.transform.position - transform.position;
          dir.y = 0;
          var rot = Quaternion.LookRotation(dir);
          transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, rotateSpeed * Time.deltaTime);
     }




}
