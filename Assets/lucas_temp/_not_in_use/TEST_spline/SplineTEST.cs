using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class SplineTEST : MonoBehaviour
{

     [Space()]
     public List<Transform> handles = new List<Transform>();
     public List<float> weight = new List<float>();

     [Range(0f, 1f)]
     public float _t;

     public float pointRadius = 0.2f;
     public float lineDot = 0.05f;
     public float lineThick = 0.01f;
     public float tThick = 0.2f;



     Vector3 GetSpline(float t)
     {
          while (weight.Count < 4)
               weight.Add(0);

          float tt = t * t;
          float ttt = tt * t;

          weight[0] = -ttt + 2 * tt - t;
          weight[1] = 3 * ttt - 5 * tt + 2;
          weight[2] = -3 * ttt + 4 * tt + t;
          weight[3] = ttt - tt;

          Vector3 result = new Vector3();
 
          int i1 = (int)t + 1;
          int i2 = i1 + 1;
          int i3 = i2 + 1;
          int i0 = i1 - 1;

          result += handles[i0].position * weight[0];
          result += handles[i1].position * weight[1];
          result += handles[i2].position * weight[2];
          result += handles[i3].position * weight[3];
          result *= 0.5f;

          return result;
     }


     void OnDrawGizmos()
     {
          if (weight == null || weight.Count < 4)
               return;

          //draw the curve
          Gizmos.color = Color.cyan;

          Gizmos.DrawWireCube(GetSpline(0.1f), new Vector3(lineThick, lineThick, lineThick));
          Gizmos.DrawWireCube(GetSpline(0.2f), new Vector3(lineThick, lineThick, lineThick));
          Gizmos.DrawWireCube(GetSpline(0.3f), new Vector3(lineThick, lineThick, lineThick));
          Gizmos.DrawWireCube(GetSpline(0.4f), new Vector3(lineThick, lineThick, lineThick));
          Gizmos.DrawWireCube(GetSpline(0.5f), new Vector3(lineThick, lineThick, lineThick));
          Gizmos.DrawWireCube(GetSpline(0.6f), new Vector3(lineThick, lineThick, lineThick));
          Gizmos.DrawWireCube(GetSpline(0.7f), new Vector3(lineThick, lineThick, lineThick));
          Gizmos.DrawWireCube(GetSpline(0.8f), new Vector3(lineThick, lineThick, lineThick));
          Gizmos.DrawWireCube(GetSpline(0.9f), new Vector3(lineThick, lineThick, lineThick));

          //draw the curve
          Gizmos.color = Color.yellow;
          Gizmos.DrawWireCube(GetSpline(_t), new Vector3(tThick, tThick, tThick));


     }







}
