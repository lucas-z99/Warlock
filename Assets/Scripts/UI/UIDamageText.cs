using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


/// <summary>
/// Display damage text.
/// </summary>
public class UIDamageText : MonoBehaviour
{

     string animName = "UIDamageTextFly";

     TextMeshProUGUI text;
     Animator anim;
     [HideInInspector] public UIDamageTextMgr mgr;
     GameObject target;
     Vector3 targetPos;


     void Awake()
     {
          anim = GetComponent<Animator>();
          text = GetComponentInChildren<TextMeshProUGUI>();
     }

     void LateUpdate()
     {
          if (target != null)
          {
               var pos = target.transform.position + new Vector3(0f, mgr.offset, 0f);
               transform.position = Camera.main.WorldToScreenPoint(pos);
          }
          else
          {
               transform.position = Camera.main.WorldToScreenPoint(targetPos);
          }
     }

     public void Display(int value, GameObject _target, Vector3 _pos, Color color)
     {
          target = _target; //follow gameObject
          targetPos = _pos; //or don't move

          _display(value, color);
     }

     float GetScale(int damageOrHealValue)
     {
          //big damage = big text
          return Mathf.LerpUnclamped(1f, UIDamageTextMgr.inst.dynamicScale, Mathf.Abs(damageOrHealValue) / (float)UIDamageTextMgr.inst.dynamicScaleAtDamage);
     }


     // private ------------------------------------------------------------------
     void _display(int value, Color color)
     {
          enabled = true;
          gameObject.SetActive(true);

          text.text = Mathf.Abs(value) + "";
          //text.color = value < 0 ? UIDamageTextMgr.inst.damageColor : UIDamageTextMgr.inst.healColor;
          text.color = color;

          var scale = GetScale(value);
          transform.localScale = new Vector3(scale, scale, scale);

          anim.Play(animName);
     }

     void OnAnimationEnd() //DO NOT change name, called by animator
     {
          enabled = false;
          gameObject.SetActive(false);

          target = null;

          mgr.RecycleUI(this);
     }



}
