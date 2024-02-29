using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

/// <summary>
/// Script attached to a projectile. Handle collision, damage, force etc.
/// </summary>
public class Projectile : MonoBehaviour
{


     bool log = false; //test

     [Header("Setting")]
     public float damageRadius = 1.5f;
     public ProjectileLauncher launcher { get; set; }


     // private
     ProjectileEntry setting { get => launcher.setting; }
     bool isMovingAndColliding = false; //if false, then it only wait for despawn
     List<GameObject> victims = new List<GameObject>();
     float tDespawn;
     Vector3 velocity;
     // network
     bool authority { get => TEST.inst.clientHasDamageAutority ? (originClientID == clientID) : NetworkManager.Singleton.IsServer; }
     ulong clientID { get => NetworkManager.Singleton.LocalClientId; }
     ulong originClientID;
     int attackerID;

     private GameObject Target;


     void Start()
     {
          Target = GameObject.FindGameObjectWithTag("Enemy");
     }


     void FixedUpdate()
     {
          if (Time.fixedTime > tDespawn)
          {
               EndOfUse();
               return;
          }

          if (!isMovingAndColliding)
               return; //eg. I'm stuck to enemy


          // reset victim list? so we can hit them again
          if (setting.hitSameTarget && Time.time > tResetVictim)
          {
               victims.Clear();
               tResetVictim = -1;
          }


          // move
          if (!setting.isHoming)
          {
               Move();
          }
          else
          {
               //Debug.Log("DSA");
               _pos0 = transform.position;
               transform.position += transform.forward * (setting.homingSpeed * Time.fixedDeltaTime);
               //Move();
               GameObject[] targets;

               targets = GameObject.FindGameObjectsWithTag("Enemy");
               //Debug.Log("IDk");
               if (Target != null)
               {
                    //Debug.Log("HI");
                    foreach (GameObject Target in targets)
                    {
                         float distance = Vector3.Distance(Target.transform.position, transform.position);

                         if (distance <= setting.homingDetect)
                         {
                              StartCoroutine(Homing());
                         }
                    }
               }
          }

          // collide
          HandleCollision();

     }


     // fire ---------------------------------------------------------------------------------
     int targetMask;
     int launcherID;

     public void Fire(ProjectilePacket packet)
     {
          // flag
          enabled = true;
          gameObject.SetActive(true);
          isMovingAndColliding = true;

          // reset
          victims.Clear();
          _hitCount = 0;

          // set up
          tDespawn = Time.time + setting.range / setting.speed;

          originClientID = packet.originClientID;
          attackerID = packet.attackerID;

          transform.position = packet.start;
          transform.rotation = Quaternion.LookRotation(packet.dir);

          targetMask = LayerMaskUtil.Get_target_mask(
               packet.team,
               setting.hitFoe,
               setting.hitFriend,
               setting.isSiege); // collision mask

          velocity = packet.dir.normalized * setting.speed;

          // VFX
          foreach (var particle in GetComponentsInChildren<ParticleSystem>())
               particle.Play();

          foreach (var trail in GetComponentsInChildren<TrailRenderer>())
               trail.Clear();
     }


     // move ---------------------------------------------------------------------------------
     void Move()
     {
          //choosing 'telerpot' over Rigidbody.Addforce() is because we want to stick the arrow on enemy,
          //but since both arrow and enemy has rb, adding force will pull them apart..
          _pos0 = transform.position;
          transform.position += velocity * Time.fixedDeltaTime;
     }


     // detect collision ---------------------------------------------------------------------------------
     Collider[] _cache = new Collider[50];
     Vector3 _pos0; //cache
     int _hitCount;
     float tResetVictim;

     void HandleCollision()
     {
          // detect wall
          var hitWall = Physics.Raycast(_pos0, transform.position - _pos0, setting.speed * Time.fixedDeltaTime, LayerMaskUtil.wall_mask);
          if (hitWall)
          {
               OnHitVFX();
               StuckToObject(null);
               return; //end of use
          }


          // detect target
          int count = Physics.OverlapCapsuleNonAlloc(_pos0, transform.position, damageRadius, _cache, targetMask);

          for (int i = 0; i < count; i++)
          {
               Collider other = _cache[i];
               int otherMask = 1 << other.gameObject.layer; //layer of the thing we hit, to layerMask

               // if target
               if ((otherMask & targetMask) != 0)
               {
                    // if not creature
                    var hpClass = other.GetComponent<HPComponent>();
                    if (hpClass == null)
                    {
                         Debug.LogError("Something in this layer but has no HP?? " + other.name);
                         continue;
                    }

                    // (!) apply constant force / every update
                    if (setting.smoothForce)
                         ApplyForce(other.gameObject, true);


                    if (hpClass.hp == 0)
                    {
                         ApplyForce(other.gameObject, true); // throwing dead enemy around
                         continue; // dead don't block bullet or take damage
                    }


                    // victim don't block bullet or take damage
                    if (victims.Contains(other.gameObject))
                         continue;

                    // good
                    _hitCount++;
                    victims.Add(other.gameObject);

                    // (!) apply instant force / only once a while
                    if (!setting.smoothForce)
                         ApplyForce(other.gameObject);

                    Damage(other.gameObject);
                    OnHitVFX();


                    // reset victim list?
                    if (setting.hitSameTarget && tResetVictim == -1)
                         tResetVictim = Time.time + setting.hitSameTargetEvery / 1000;


                    // finally
                    if (setting.maxHit > 0 && _hitCount >= setting.maxHit)
                    {
                         StuckToObject(other.transform);
                         return; //end of use
                    }
               }

          }

     }

     void StuckToObject(Transform target)
     {
          // stick to wall or enemy
          isMovingAndColliding = false;
          tDespawn = Time.fixedTime + (target == null ? setting.stickToWall : setting.stickToTarget);

          if (target)
               transform.parent = target;  // stick to enemy
     }


     // on hit ---------------------------------------------------------------------------------

     void OnHitVFX() //visual effect
     {
          if (log) Debug.Log("OnHitVFX()");

          if (setting.onHitVFX == null)
               return;

          var vfx = launcher.GetVFX(this);
          vfx.gameObject.SetActive(true);
          vfx.transform.position = transform.position;

          // in case you have particles
          var pList2 = vfx.GetComponentsInChildren<ParticleSystem>(); //this DOES includes <T> on the parent
          if (pList2.Length > 0)
               foreach (var p in pList2)
                    p.Play();

          // finally
          launcher.PlanRecycleVFX(vfx);

     }


     void Damage(GameObject target)
     {
          if (log) Debug.Log("Damage()");

          if (!authority)
               return;

          int damageOrHeal = setting.damage + Random.Range(-setting.dmgRandomRange, setting.dmgRandomRange + 1);

          var hpClass = target.GetComponent<HPComponent>();
          if (setting.isHeal)
               hpClass.Heal(damageOrHeal);
          else
               hpClass.Damage(damageOrHeal, attackerID, setting.isSiege);
     }


     void ApplyForce(GameObject target, bool smooth = false)
     {
          if (log) Debug.Log("Knock()");

          if (!NetworkManager.Singleton.IsServer) //sadly we can't let client handle force, Pos is sync by Server
               return;

          var rb = target.GetComponent<Rigidbody>();
          if (!rb)
               return;

          // good
          var force = new Vector3();
          if (setting.forceDirection == ForceDir.Foward)
          {
               force = transform.forward * setting.forceFwdUp.x + Vector3.up * setting.forceFwdUp.y;
          }
          else if (setting.forceDirection == ForceDir.AwayFromCenter)
          {
               var dirXZ = (target.transform.position - transform.position).normalized;
               dirXZ.y = 0; //chara's Y=0, but bullet has Y>0, this cause unintended up-suck force
               force = dirXZ * setting.forceFwdUp.x + Vector3.up * setting.forceFwdUp.y;
          }

          force *= smooth ? Time.fixedDeltaTime : 1; //impulse or constant
          Debug.Log("force = " + force + "  |  mag = " + force.magnitude);

          // finally
          //var ai = target.GetComponent<AIBrain>();
          //if (ai)
          //     ai.Apply_force(force);
          //else
          //     rb.AddForce(force, ForceMode.Impulse);

          rb.AddForce(force, ForceMode.Impulse);

     }


     // end of use ---------------------------------------------------------------------------------
     public void EndOfUse()
     {
          if (log) Debug.Log("EndOfUse()");
          isMovingAndColliding = false;
          gameObject.SetActive(false);

          victims.Clear();
          transform.parent = launcher.transform;

          launcher.Recycle(this);
     }


     // show collider ---------------------------------------------------------------------------------
     void OnDrawGizmosSelected()
     {
          if (Application.isPlaying)
          {
               Gizmos.color = Color.blue;
               Gizmos.DrawWireSphere(_pos0, damageRadius);
          }

          Gizmos.color = Color.red;
          Gizmos.DrawWireSphere(transform.position, damageRadius);
     }


     //Homing -------------------------------------------------------------------
     IEnumerator Homing()
     {
          //time until looking for closest enemy
          yield return new WaitForSeconds(setting.homingDelay);

          FindClosestEnemy();


     }
     public GameObject FindClosestEnemy()
     {
          GameObject[] gos;
          gos = GameObject.FindGameObjectsWithTag("Enemy");
          GameObject closest = null;
          float distance = Mathf.Infinity;
          Vector3 position = transform.position;
          foreach (GameObject go in gos)
          {
               Vector3 diff = go.transform.position - position;
               float curDistance = diff.sqrMagnitude;
               if (curDistance < distance)
               {

                    closest = go;
                    distance = curDistance;
               }


          }

          if (closest != null)
          {
               //move towards closest enemy
               Vector3 direction = (closest.transform.position - transform.position).normalized;
               Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
               transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * setting.homingRotateSpeed);
          }


          return closest;
     }
}

