using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TEST_G : MonoBehaviour
{


     public GameObject basic_force;
     public GameObject basic_impulse;
     public GameObject basic_set_velocity;
     public GameObject fake_physics;


     [Space(20)]
     public float up_force = 20f;
     public float tForce = 0.2f;

     [Space(20)]
     public float high1;
     public float high2;
     public float high3;
     public float high4;
     public bool click_to_reset;

     public int frames;
     public Vector3 fakeVel;

     bool keydown;
     float tEnd;


     void Update()
     {
          if (Input.GetKeyDown(KeyCode.Alpha1))
          {
               keydown = true;
          }
     }


     void FixedUpdate()
     {
          var force = new Vector3(0, up_force, 0);

          if (keydown)
          {
               keydown = false;

               // 1
               tEnd = Time.time + tForce;

               // 2
               basic_impulse.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);

               // 3
               basic_set_velocity.GetComponent<Rigidbody>().velocity = force;

               // 4
               fakeVel += force;
          }


          // 2 continue
          if (Time.time < tEnd)
          {
               basic_force.GetComponent<Rigidbody>().AddForce(force, ForceMode.Force);
               frames++;
          }


          // 4 continue
          fake_physics.transform.position += fakeVel * Time.deltaTime;

          if (fake_physics.transform.position.y < 0)
          {
               var x = fake_physics.transform.position.x;
               var z = fake_physics.transform.position.z;
               fake_physics.transform.position = new Vector3(x, 0, z);
          }

          if (fake_physics.transform.position.y > 0)
          {
               fakeVel += Physics.gravity * Time.deltaTime;
          }
          else
          {
               fakeVel = Vector3.zero;
          }


          //
          record_high();
     }

     void record_high()
     {
          if (click_to_reset)
          {
               click_to_reset = false;
               high1 = high2 = high3 = high4 = frames = 0;
          }


          if (high1 < basic_force.transform.position.y)
               high1 = basic_force.transform.position.y;

          if (high2 < basic_impulse.transform.position.y)
               high2 = basic_impulse.transform.position.y;

          if (high3 < basic_set_velocity.transform.position.y)
               high3 = basic_set_velocity.transform.position.y;

          if (high4 < fake_physics.transform.position.y)
               high4 = fake_physics.transform.position.y;

     }

}
