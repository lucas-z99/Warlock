using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;


public enum DropMode
{
     EverythingIsPossible, //Roll every item independently
     EverythingIsPossible_ButNeverNothing, //If all fail, use weighted probability to roll 1 item
     OneAmongAll, //ONLY and ALWAYS drop 1 item. Use weighted probability
}


[CreateAssetMenu(menuName = "3770/DropTable")]
[ExecuteInEditMode]
public class DropTable : ScriptableObject
{

     [Tooltip("Delete duplicated, null or empty entry")]
     public bool clickMeToTrim;
     public DropMode Probability = DropMode.EverythingIsPossible;
     public List<DropEntry> entries;


     void OnValidate()
     {
          if (clickMeToTrim)
          {
               clickMeToTrim = false;
               Trim();
          }
     }

     [HideInInspector] public bool trim;
     public void Trim()
     {
          //this structure appears in inspector, who knows what user'd do?

          entries = entries.DistinctBy(e => e.obj).ToList(); //remove duplicate

          for (int i = entries.Count - 1; i >= 0; i--)
               if (entries[i].obj == null)
                    entries.RemoveAt(i); //remove nullD

          entries.TrimExcess(); //trim

          trim = true;
     }

}


[Serializable]
public class DropEntry
{
     public GameObject obj; //prefab

     [Range(0, 100)]
     public int chance; //percentage
}


