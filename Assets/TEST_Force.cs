using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;



public class TEST_Force : MonoBehaviour
{


     //public float maxAcc = 100f;

     public float speedToAcc = 10;

     public float maxSpeed = 10f;
     public float boomStrength = 1000f;
     public float boomHeight = 1000f;


     [Header("Moniter")]
     public Vector3 rb_velocity;
     public float rb_velocity_mag;
     public float DIST;


     float inputH;
     float inputV;
     Rigidbody rb;
     Vector3 force;
     Vector3 boomForce;
     Vector3 distMeasureStart;


     void Awake()
     {
          rb = GetComponent<Rigidbody>();
     }

     void Update()
     {
          inputH = Input.GetAxisRaw("Horizontal");
          inputV = Input.GetAxisRaw("Vertical");

          if (Input.GetKeyDown(KeyCode.Mouse1))
          {
               boom = true;
               var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
               if (Physics.Raycast(ray, out var hit))
               {
                    boomDir = (hit.point - transform.position).normalized; //pretend we hit the skybox
                    boomDir.y = 0;
               }
          }

          if (Input.GetKeyDown(KeyCode.Space))
               distMeasureStart = transform.position;
     }

     bool boom;
     Vector3 boomDir;

     [Space(20)]
     public Vector3 velDesired;
     public Vector3 acc;
     float magic = 0.55f; //since we are lerping, and with friction, +1 is needed to reach maxSpeed


     void FixedUpdate()
     {
          //debug
          rb_velocity = rb.velocity;
          rb_velocity_mag = rb_velocity.magnitude;
          DIST = Vector3.Distance(transform.position, distMeasureStart);

          //vel
          if (inputH != 0 || inputV != 0)
          {
               //hasInput = true;
               velDesired = (inputH * transform.right + inputV * transform.forward).normalized * (maxSpeed + magic);
               acc = (velDesired - rb.velocity) / Time.fixedDeltaTime;
               acc.y = 0;
               acc = Vector3.ClampMagnitude(acc, maxSpeed * speedToAcc);
               rb.AddForce(acc, ForceMode.Acceleration);
          }

          //boom
          if (boom)
          {
               boom = false;

               boomForce = boomDir * boomStrength + new Vector3(0, boomHeight, 0);
               rb.AddForce(boomForce, ForceMode.Acceleration);
          }

     }


     public float gizmosSize = 2f;

     void OnDrawGizmos()
     {
          Gizmos.color = Color.red;
          Gizmos.DrawLine(transform.position, transform.position + boomForce * gizmosSize);
          Gizmos.DrawWireSphere(transform.position + boomForce * gizmosSize, 0.23f);

          Gizmos.color = Color.cyan;
          Gizmos.DrawLine(transform.position, transform.position + force * gizmosSize);
          Gizmos.DrawWireSphere(transform.position + force * gizmosSize, 0.25f);

          Gizmos.color = Color.white;
          Gizmos.DrawLine(transform.position, transform.position + rb_velocity * gizmosSize);
          Gizmos.DrawWireSphere(transform.position + rb_velocity * gizmosSize, 0.25f);
     }



}
