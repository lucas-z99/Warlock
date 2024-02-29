using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;


/// <summary>
/// Not AI. But control hydra's head and body
/// </summary>
[ExecuteInEditMode]
public class Hydra : MonoBehaviour
{
     public bool gizmos; //debug

     public Transform mouth; // AI will move this part around
     public Transform head; // the fist spine, follows the mouth with a offset
     public Transform root; // the last spine, not moving
     public float max_dist = 20f;

     public List<Transform> bodys; //everything in between


     void Awake()
     {
          Init();
     }

     void LateUpdate()
     {
          Constrain_movement();
          Update_body();
     }

     void Init()
     {
          bodys = new List<Transform>();
          bodys.AddRange(transform); //add all children

          bodys.Remove(mouth);
          bodys.Remove(head);
          bodys.Remove(root);


          if (bodys.Count < 3)
               Debug.LogError("must have >= 3 children : head -> body(s) -> root");
     }


     // constrain head  -----------------------------------------------------------------------------------------

     void Constrain_movement()
     {
          if (Vector3.Distance(mouth.position, root.position) > max_dist)
          {
               var dir = mouth.position - root.position;
               mouth.position = root.position + dir.normalized * max_dist;
          }
     }


     // body move to catch up  -----------------------------------------------------------------------------------------
     public float catch_up_lerp = 1.5f; //curve the body movement
     public float suspend_after = 0.1f; //suspend the body curve after X sec
     Vector3 pos0;
     float tLastHeadMove;

     void Update_body()
     {
          if (pos0 != mouth.position)
          {
               pos0 = mouth.position;
               tLastHeadMove = Time.time;
          }

          if (Time.time < tLastHeadMove + suspend_after)
               for (int i = 0; i < bodys.Count; i++)
               {
                    var desiredPos = Vector3.Lerp(head.position, root.position, (float)(i + 1) / (bodys.Count + 1));
                    //in the lerp, both i and count +=1 because head is before bodys, and root is after bodys

                    var lerp = Mathf.Pow(catch_up_lerp, (bodys.Count - i)); //power seems most snake-like

                    bodys[i].position = Vector3.Lerp(bodys[i].position, desiredPos, lerp * Time.deltaTime);
               }
     }


     // debug  -----------------------------------------------------------------------------------------
     void OnValidate()
     {
          Init();
     }

     void OnDrawGizmos()
     {
          if (!gizmos)
               return;

          Gizmos.color = Color.yellow;
          Gizmos.DrawWireSphere(root.position, max_dist);

          for (int i = 1; i < bodys.Count; i++)
          {
               Gizmos.DrawLine(bodys[i].position, bodys[i - 1].position);
          }
     }









}
