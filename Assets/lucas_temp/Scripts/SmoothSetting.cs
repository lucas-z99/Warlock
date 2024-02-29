using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SmoothSetting : MonoBehaviour
{
     public static SmoothSetting inst;

     [Header("Mode / Shared Setting")]
     public ClientSmoothMode mode;
     public float flatVelocity = 1; //a small movement to make-up micro differences
     public float extrapolation = 1.25f; //1 = origin
     public float maxDeviation = 10f; //teleport if greater then this 

     [Header(" - Default Mode")]
     public float smoothTime = 0.3f;

     [Header(" - New SpeedCap Mode")]
     public float speedCapX = 1.1f; //same to default, but speed cap at X times speed of chara/enemy

     [Header(" - Lerp Mode")]
     public float posLerp = 2f;

     [Header("Rotation")]
     public float rotLerp = 10;

     void Awake()
     {
          inst = this;
     }

}

public enum ClientSmoothMode
{
     Default,
     SpeedCap,
     Lerp,
}