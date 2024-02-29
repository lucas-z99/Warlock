using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// Efficiency! Avoid 1 Update() on all 10000 GameObjects
/// </summary>
public class UpdateCaller : MonoBehaviour
{

     void Awake()
     {
          if (_inst != null && _inst != this)
          {
               Debug.Log("singleton plz! remove duplicate");
               enabled = false;
          }
     }
     void Update()
     {
          update_face_camera();
     }


     // singleton  ----------------------------------------------------
     public static UpdateCaller inst { get => get_singleton(); }
     static UpdateCaller _inst;
     static UpdateCaller get_singleton()
     {
          if (_inst == null)
          {
               var found = FindObjectsOfType<UpdateCaller>();
               if (found.Length > 0) //find existing
               {
                    _inst = found[0];
               }
               else //make a new
               {
                    var obj = new GameObject("UpdateCaller");
                    _inst = obj.AddComponent<UpdateCaller>();
               }
          }

          return _inst;
     }


     // update face camera  ----------------------------------------------------

     public List<FaceCamera> face_camera = new List<FaceCamera>();
     void update_face_camera()
     {
          for (int i = face_camera.Count - 1; i >= 0; i--)
          {
               var thing = face_camera[i];

               if (thing != null)
                    thing.transform.LookAt(thing.transform.position + Camera.main.transform.forward);
               else
                    face_camera.Remove(thing);
          }
     }






}
