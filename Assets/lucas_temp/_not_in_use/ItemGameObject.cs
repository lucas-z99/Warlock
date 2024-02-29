using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Item on the ground, waiting for pick up
[ExecuteInEditMode]
public class ItemGameObject : MonoBehaviour, IInteractable
{

     public string itemName;
     public string description;

     public Text ui_name; //temp
     public Text ui_description; //temp

     // private
     [HideInInspector] public DropManager mgr;
     Collider col;

     void Start()
     {
          col = GetComponent<Collider>();
          col.isTrigger = true;

          gameObject.layer = LayerMask.NameToLayer("Item");
     }
     void OnValidate()
     {
          if (ui_name)
               ui_name.text = itemName;
          if (ui_description)
               ui_name.text = description;
     }
     void OnTriggerEnter(Collider other)
     {
          Debug.Log("OnTriggerEnter " + other.name);

          gameObject.SetActive(false);
     }



     // interface
     string IInteractable.nameTag => throw new NotImplementedException();
     string IInteractable.InteractName => throw new NotImplementedException();
     Action IInteractable.Interact()
     {
          throw new NotImplementedException();
     }






}
