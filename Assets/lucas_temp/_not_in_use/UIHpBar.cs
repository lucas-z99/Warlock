using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;



public class UIHpBar : MonoBehaviour
{

     //public  ----------------------------------------------------------------------

     public float smooth = 0.5f; // 0 = no smooth

     public void StartUse(GameObject followThisObject, HPComponent hpClass, float height = 4f, bool isElite = false)
     {
          SetFollow(followThisObject, height);
          this.hpClass = hpClass;

          if (ui_eliteFlag) ui_eliteFlag.enabled = isElite;

          gameObject.SetActive(true);
          enabled = true;
     }

     public void Recycle()
     {
          gameObject.SetActive(false);
          enabled = false;
          transform.SetParent(null);

          UIHpBarFactory.Recycle(gameObject);
     }

     public void RefreshUINow() //with no smooth
     {
          UpdateUI(0);
     }



     // private  ----------------------------------------------------------------------
     [SerializeField] Text ui_text;
     [SerializeField] Text ui_textDead;
     [SerializeField] Text ui_eliteFlag;
     [SerializeField] Image hpBar;

     //update
     HPComponent hpClass;
     float hp { get => hpClass.hp; }
     float hpMax { get => hpClass.maxHP; }
     float hpSmooth;
     float minStep = 1;

     void LateUpdate()
     {
          if (hpClass == null)
               return;

          //face camera
          transform.LookAt(transform.position + Camera.main.transform.forward);

          UpdateUI(smooth);
     }

     void UpdateUI(float _smooth)
     {
          if (_smooth < 0 || hp == hpMax)
          {
               hpSmooth = hp;
          }
          else
          {
               hpSmooth = Mathf.MoveTowards(hpSmooth, hp, minStep * Time.deltaTime);
               hpSmooth = Mathf.Lerp(hpSmooth, hp, smooth * Time.deltaTime);
          }

          if (ui_text)
               ui_text.text = (int)hpSmooth + "";
          if (hpBar)
               hpBar.transform.localScale = new Vector3(hpSmooth / hpMax, 1, 1);

          if (ui_textDead)
               ui_textDead.enabled = (hpSmooth == 0);

     }


     //follow gameobject
     GameObject follow;
     Vector3 offset = new Vector3(0, 4f, 0);

     void SetFollow(GameObject obj, float height)
     {
          transform.SetParent(obj.transform);
          transform.localPosition = new Vector3(0, height, 0);
     }


}


