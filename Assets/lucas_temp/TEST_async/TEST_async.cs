using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class TEST_async : MonoBehaviour
{

     void OnGUI()
     {
          if (GUILayout.Button("Coroutine"))
          {
               Debug.Log("F1");
               StartCoroutine(BunchOfCoroutine());
               StartCoroutine(BunchOfCoroutine(1));
               BunchOfCoroutine(); //won't work. not even the debug line??
               Debug.Log("F2"); //won't be blocked
          }
          if (GUILayout.Button("Async_Delay"))
               Async_Delay(2);
          if (GUILayout.Button("Async_Yield"))
               Async_TaskYield(2);
          if (GUILayout.Button("Async_RunAction"))
               Async_RunAction();



     }

     #region

     IEnumerator BunchOfCoroutine(float delay = 0)
     {
          Debug.Log("BunchOfCoroutine()");

          yield return new WaitForSeconds(delay);

          yield return StartCoroutine(Something1());
          yield return StartCoroutine(Something2());
          yield return StartCoroutine(Something3());
     }

     IEnumerator Something1()
     {
          yield return new WaitForSeconds(2);
          Debug.Log("Something1");
     }
     IEnumerator Something2()
     {
          yield return new WaitForSeconds(2);
          Debug.Log("Something2");
     }
     IEnumerator Something3()
     {
          yield return new WaitForSeconds(2);
          Debug.Log("Something3");
     }

     //IEnumerator CPrint(string text, float delay)
     //{
     //     //Debug.Log("");
     //     yield return new WaitForSeconds(delay);
     //     Debug.Log(text);
     //}

     #endregion

     public async void Async_Delay(int delay_ms)
     {
          Debug.Log("F1 " + Time.frameCount);
          await Task.Delay(delay_ms);
          Debug.Log("F2 " + Time.frameCount);
     }

     public async void Async_TaskYield(int loopSkipped)
     {
          var frame = Time.frameCount + loopSkipped;
          int loop = 0;
          while (Time.frameCount < frame)
          {
               int f0 = Time.frameCount;
               await Task.Yield(); // <- wait 1 frame
               int f1 = Time.frameCount;

               loop++;

               Debug.Log("F = " + f0 + " | " + f1 + " | " + (f0 == f1));
          }
          Debug.Log("loop = " + loop); // interestingly, there seems to be always a +1 while loop
     }

     public async void Async_RunAction()
     {
          Debug.Log("F1 " + Time.frameCount);
          //var t = Time.time;
          await Task.Run(() => WaitForSec(3)); // <- cannot call Time.time, no longer in main thread
          Debug.Log("F2 " + Time.frameCount);
     }

     async void WaitForSec(float delay)
     {
          Debug.Log("Wait3Sec");
          var tContinue = delay + 3;
          while (Time.time < tContinue)
          {
               await Task.Yield();
          }
          Debug.Log("Good");
     }

     public async void Async_GettingWeird(string text, int delay_ms)
     {
          Debug.Log("F1");

          Task<string>[] dummy = null;


          await Task.WhenAny();
          await Task.WhenAll(dummy);

          var fun = Task.WaitAny(dummy);
          Task.WaitAll(dummy);

          Debug.Log("F2");
     }

}
