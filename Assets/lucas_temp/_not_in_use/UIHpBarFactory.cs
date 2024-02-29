using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


/// <summary>
/// Distribute hp bar. The UI will follow the gameobject
/// 
/// To use: 
/// add HPComponent to GameObject, or implement IDamageAble on your hp class
/// UIHpBarFactory.GetUI() for a UI
/// UIHpBarFactory.Recycle() to recycle
///
/// </summary>
public class UIHpBarFactory : MonoBehaviour
{

     // public  ----------------------------------------------------------------------

     public static UIHpBar GetUI()
     {
          Singleton();
          return pool.Get();
     }
     public static void Recycle(GameObject obj)
     {
          var ui = obj.GetComponent<UIHpBar>();

          if (!ui)
          {
               Debug.LogError("This GameObject is not a hp bar");
               return;
          }

          ui.transform.SetParent(sing.transform);

          pool.Release(ui);
     }


     // private  ----------------------------------------------------------------------

     //singleton
     static UIHpBarFactory sing;
     void Awake()
     {
          if (sing == null)
               sing = this;
          else
          {
               Debug.LogError("Only 1 of this class should exist");
               enabled = false;
               return;
          }

          InitPool();
     }
     static void Singleton()
     {
          if (sing)
               return;

          var obj = new GameObject();
          obj.AddComponent<UIHpBarFactory>();
     }


     //pool
     GameObject UIPrefab { get => Instantiate(Resources.Load("UIHpBar")) as GameObject; }
     static ObjectPool<UIHpBar> pool;
     void InitPool()
     {
          var size = 200;
          var sizeCap = 500;
          pool = new ObjectPool<UIHpBar>(CreateNewUI, null, null, null, false, size, sizeCap);
     }
     static UIHpBar CreateNewUI()
     {
          var gameObject = Instantiate(sing.UIPrefab, sing.transform);
          gameObject.SetActive(false);

          var ui = gameObject.GetComponent<UIHpBar>();

          return ui;
     }


}


