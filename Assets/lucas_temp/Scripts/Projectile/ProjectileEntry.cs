using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "3770/ProjectileEntry")]
public class ProjectileEntry : ScriptableObject
{
     // data for Projectile

     [Header(" - Basic")]
     public Projectile prefab;
     public float speed = 70;
     public float range = 100;
     public int damage = 1;
     public int maxHit = 1;


     [Header(" - Launch")]
     public TriggerMode triggerMode = TriggerMode.KeyDown;
     public Vector2 spawnFwdUp = new Vector2(1.5f, 2f);
     public int delay = 20; //ms before launch
     public int cooldown = 150; //ms before launch again


     [Header(" - Force")]
     public Vector2 forceFwdUp = new Vector2(100, 33);
     public ForceDir forceDirection = ForceDir.Foward;
     public bool smoothForce = false;


     [Header(" - Homing")]
     public bool isHoming;
     public float homingDetect = 10f;  //Detection range for Enemies
     public float homingSpeed = 11.0f;
     public float homingRotateSpeed = 5.0f;
     public float homingDelay;


     [Header(" - Visual")]
     public float stickToTarget = 30f;
     public float stickToWall = 30f;
     public GameObject onHitVFX; //prefab
     public int recycleVFX = 2000; //ms, recycle after X second, TODO: maybe there is a better way?


     [Header(" - AoE")]
     public bool hitSameTarget = false; // can we hit the same targets repeatedly? eg. a fire wall
     public float hitSameTargetEvery = 1000; //ms



     [Header(" - Other")]
     public bool hitFoe = true;
     public bool hitFriend = false;  // use object's layer (player/enemy) as its team
     public bool isHeal = false;
     public bool isSiege = false; // some spell can damage soft terrain

     public int dmgRandomRange = 0; // -this < x < this. Randomly change damage by x
     public float speedWhenFiring = 100; //move speed during pre-fire delay, percentage, only for player, Note: with friction a very low speed = not moving at all
     public int burst = 1; //how many projectiles to fire at once, when >1 this will fire in all directions

     [Range(0, 89)] public float maxUpwardsAgnle = 30; //can we fire up/downwards?
     [Range(0, 89)] public float maxDownwardsAgnle = 0;

     public int preheatPool = 0; // pre heat the pool so maybe we lag less






}


public enum TriggerMode
{
     KeyDown = 1,
     KeyUp = 2,
     FullyAuto = 3,
}

public enum ForceDir
{
     Foward = 1, //eg. push(+) or pull(-), using projectile's direction
     AwayFromCenter = 2, //eg. push(+) or suck(-), RELATIVE to the projectile center
}


