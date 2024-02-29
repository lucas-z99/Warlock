using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FaceCamera : MonoBehaviour
{
     void Awake()
     {
          UpdateCaller.inst.face_camera.Add(this);
     }

     public void End_of_use()
     {
          UpdateCaller.inst.face_camera.Remove(this);
     }
}
