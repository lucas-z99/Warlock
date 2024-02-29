using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


[CreateAssetMenu(menuName = "3770/UIDamageTextMgr")]
public class UIDamageTextMgr : ScriptableObject
{

     // public
     public UIDamageText prefab;
     public Color damageColor = Color.white;
     public Color damageYouColor = Color.red;
     public Color healColor = Color.green;
     public float offset = 4.25f; //above head
     public float dynamicScale = 3;
     public int dynamicScaleAtDamage = 100; // big damage = big text


     public static void DisplayDamageText(int value, GameObject obj, bool targetIsMe)
     {
          var ui = inst.pool.Get();
          ui.Display(value, obj, Vector3.zero, GetColor(value, targetIsMe));
     }
     public static void DisplayDamageText(int value, Vector3 pos, bool targetIsMe)
     {
          var ui = inst.pool.Get();
          ui.Display(value, null, pos, GetColor(value, targetIsMe));
     }

     public static void Init()
     {
          // call this to decrease (maybe?) lag when first hit enemy
          // TODO: but why?? Resources.Load?
          inst.init();
     }


     // initial & singleton  --------------------------------------------------------------------------------- 
     public static UIDamageTextMgr inst { get => GetSingleton(); }
     static UIDamageTextMgr _inst;

     void init()
     {
          if (pool == null)
               inst.InitPool();
     }

     static UIDamageTextMgr GetSingleton()
     {
          if (_inst == null)
          {
               _inst = Resources.Load(typeof(UIDamageTextMgr).Name) as UIDamageTextMgr;
               _inst.init();
          }
          return _inst;
     }


     // pool  ---------------------------------------------------------------------------------
     ObjectPool<UIDamageText> pool;
     void InitPool()
     {
          var size = 200;
          var sizeCap = 500;
          pool = new ObjectPool<UIDamageText>(CreateNewUI, null, null, null, false, size, sizeCap);
     }

     UIDamageText CreateNewUI()
     {
          var obj = Instantiate(prefab.gameObject, canvas.transform);
          var ui = obj.GetComponent<UIDamageText>();

          obj.SetActive(false);
          ui.enabled = false;
          ui.mgr = this;

          return ui;
     }

     public void RecycleUI(UIDamageText ui)
     {
          ui.enabled = false;
          ui.gameObject.SetActive(false);

          pool.Release(ui);
     }


     // UI  --------------------------------------------------------------------------------- 
     Canvas canvas { get => inst.GetOrCreateCanvas(); }
     Canvas _canvas;

     Canvas GetOrCreateCanvas()
     {
          if (_canvas != null)
               return _canvas;


          // or find a ScreenSpaceOverlay canvas
          var all = FindObjectsByType<Canvas>(FindObjectsSortMode.None);

          foreach (var c in all)
               if (c.renderMode == RenderMode.ScreenSpaceOverlay)
                    return _canvas = c;

          // or create one
          var obj = new GameObject();
          var newCanvas = obj.AddComponent<Canvas>();
          newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
          return _canvas = newCanvas;

     }

     static Color GetColor(int value, bool targetIsMe)
     {
          if (value > 0)
               return inst.healColor;

          if (targetIsMe)
               return inst.damageYouColor;

          return inst.damageColor;
     }

}
