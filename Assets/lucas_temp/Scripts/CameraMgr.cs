using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;

[ExecuteInEditMode]
public class CameraMgr : MonoBehaviour
{

     public static CameraMgr inst;

     public Camera camMain;
     public Camera camSpectator;

     public bool lockRotation;

     public Vector3 offset = new Vector3(0, 2f, 0);
     public float dist = -20;
     [Range(1, 180)]
     public int FOV = 65;
     public Vector2 clampAngle = new Vector2(0, 90);
     public float rotateSpeed = 5;

     //private
     Transform root { get => camMain.transform.parent; } //we move,rorate root obj, and main camera is a child of this
     float rotX;
     float rotY;
     bool specMode;



     void Awake()
     {
          inst = this;
     }

     void Start()
     {
          SetSpectatorMode(false);

          rotX = root.transform.eulerAngles.x;
          rotY = root.transform.eulerAngles.y;
          dist = camMain.transform.localPosition.z;
     }
     void OnValidate()
     {
          UpdateMainCam(); ;
     }
     void Update()
     {
          UpdateMainCam();
     }

     void UpdateMainCam()
     {
          // roration
          if (!lockRotation && Input.GetMouseButton(1))
          {
               rotX -= Input.GetAxis("Mouse Y") * rotateSpeed;
               rotY += Input.GetAxis("Mouse X") * rotateSpeed;

               rotX = Mathf.Clamp(rotX, clampAngle.x, clampAngle.y); //clamp so cam don't go underground

               root.eulerAngles = new Vector3(rotX, rotY, 0);
          }

          // position - apply to root
          if (NetworkChara.myChara != null)
               root.position = NetworkChara.myChara.transform.position + offset;

          // distance to middle parent - apply to camera
          camMain.transform.localPosition = new Vector3(0, 0, dist);
          camMain.fieldOfView = FOV;
     }

     public void SetSpectatorMode(bool flag)
     {
          specMode = flag;

          camMain.gameObject.SetActive(!flag);
          camSpectator.gameObject.SetActive(flag);
     }

     //public void ToggleSpectatorMode()
     //{
     //     SetSpectatorMode(!specMode);
     //}


}
