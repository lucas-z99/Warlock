using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerSetting : MonoBehaviour
{

     public static PlayerSetting inst { get { if (_inst == null) _inst = FindObjectOfType<PlayerSetting>(); return _inst; } }
     static PlayerSetting _inst;


     public float maxSpeed = 8.8f;
     public float maxSpeedRun = 13.2f;
     public float accStrength = 8;
     public float accStrengthAir = 3;

     public float rotateLerp = 17.5f;

     public float jumpVelocity = 10f;
     public int jumpCount = 2;



}
