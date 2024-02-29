using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITargetData
{
     public HPComponent hp;
     public Transform transform { get => hp.transform; }
     public float dist;

}

