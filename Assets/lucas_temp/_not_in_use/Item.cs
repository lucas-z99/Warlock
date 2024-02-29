using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Item : MonoBehaviour
{
     readonly static int collisionMasl; //collide with player
     //public ItemCard card; //detail of this item

     //private
     SpriteRenderer ren;
     Collider col;

     void Awake()
     {
          SetupColloder();
          UpdateImage();
     }
     void OnValidate()
     {
          UpdateImage();
     }

     // setup collider
     void SetupColloder()
     {
          col = GetComponent<Collider>();
          col.isTrigger = true;
          //gameObject.layer = LayerMask.NameToLayer("Item");
     }

     // 2d image
     void UpdateImage()
     {
          //if (card == null) return;

          //ren = GetComponentInChildren<SpriteRenderer>();
          //if (ren != null) ren.sprite = card.image;
     }

     void OnTriggerEnter(Collider other)
     {
          //Debug.Log("OnTriggerEnter");
     }

}
