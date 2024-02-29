using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class LazerBeam : MonoBehaviour
{

     public bool TEST_MouseLazer;


     float maxDist = 500;
     LineRenderer lazer;
     bool inUse;
     int rayCastMask;
     Vector3 from;
     Vector3 dir;
     Vector3 to;
     ParticleSystem OnHitParticle;
     bool useDir;
     Action<Vector3> onHit;


     void Start()
     {
          lazer = GetComponent<LineRenderer>();
          OnHitParticle = GetComponentInChildren<ParticleSystem>(true);
          rayCastMask = LayerMaskUtil.Get_target_mask(CharaTeam.enemy, true, false, true);
          rayCastMask |= LayerMaskUtil.wall_mask;
     }
     void Update()
     {
          if (inUse)
               UpdateLazer();

          if (TEST_MouseLazer)
               TEST_ShootWithMouse();
     }

     public void ShootAtDir(Vector3 _from, Vector3 _dir, Action<Vector3> _onHit = null)
     {
          inUse = true;
          from = _from;
          onHit = _onHit;

          dir = _dir;
          useDir = true;
     }

     public void ShootAtPos(Vector3 _from, Vector3 _to, Action<Vector3> _onHit = null)
     {
          inUse = true;
          from = _from;
          onHit = _onHit;

          to = _to;
          useDir = false;
     }

     void UpdateLazer()
     {
          var _dir = useDir ? dir : (to - from);
          var ray = new Ray(from, _dir);
          bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, maxDist, rayCastMask);

          //lazer
          lazer.enabled = true;
          lazer.SetPosition(0, from);
          var p2 = hit ? hitInfo.point : _dir.normalized * maxDist;
          lazer.SetPosition(1, p2);


          //particle
          OnHitParticle.gameObject.SetActive(hit);
          if (hit)
          {
               OnHitParticle.transform.position = hitInfo.point;
               if (!OnHitParticle.isPlaying)
                    OnHitParticle.Play();

               onHit?.Invoke(hitInfo.point);
          }

     }

     public void Stop()
     {
          inUse = false;
          lazer.enabled = false;
          OnHitParticle.gameObject.SetActive(false);
     }



     // private ------------------------------------------------


     void TEST_ShootWithMouse()
     {
          var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
          var hit = Physics.Raycast(ray, out RaycastHit hitInfo);

          if (hit)
               ShootAtPos(transform.position, hitInfo.point);
          else
               Stop();
     }

     void OnDrawGizmos()
     {
          Gizmos.color = Color.green;
          Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 1));
     }


}
