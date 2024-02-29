using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Item on the ground, trap in the swamp, NPC in the village, or Decapitated Priest in Diablo.
/// </summary>
public interface IInteractable
{
     public string nameTag { get; }

     public string InteractName { get; }
     public Action Interact(); //pick up, open, talk, loot, disarm, interact
}

