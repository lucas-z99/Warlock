using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TEST_BulletTrail : MonoBehaviour
{

     public Vector3 p1 = new Vector3(0, 2, 0);
     public Vector3 p2 = new Vector3(0, 2, 10);
     public Vector3 p3 = new Vector3(10, 2, 10);
     public Vector3 p4 = new Vector3(10, 2, 0);
     public float speed = 6f;

     [Space()]
     public Vector3 pos;
     public Vector3 next;


     void Start()
     {
          transform.position = p1;
     }

     void FixedUpdate()
     {
          if (transform.position == p1)
               next = p2;
          else if (transform.position == p2)
               next = p3;
          else if (transform.position == p3)
               next = p4;
          else if (transform.position == p4)
               next = p1;

          transform.position = Vector3.MoveTowards(transform.position, next, speed * Time.deltaTime);

          pos = transform.position;
     }





}
