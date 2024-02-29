using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary; //save to binary
using System.Xml.Serialization; //save to xml


/// <summary>
/// Save data in to binary. See Example()
/// </summary>
public class IO
{

     public static bool log = true; //debug
     public static string defaultPath = Application.persistentDataPath + "/";


     // public
     public static void SaveData<T>(string path, T content)
     {
          _SaveData(path, content);
     }
     public static T LoadData<T>(string path) where T : class
     {
          return _LoadData<T>(path);
     }


     // example ------------------------------------------------------------------------------------
     public static void Example()
     {
          string fileName = "test.data";

          //save
          var data = new DummyData();

          data.number = 100;
          data.list = new List<string>();
          data.list.Add("this list will also be saved");
          data.date = DateTime.Now;

          SaveData(defaultPath + fileName, data);

          //load
          var loadedData = LoadData<DummyData>(defaultPath + fileName);

          Debug.Log(loadedData.number);
          Debug.Log(loadedData.list[0]);
          Debug.Log(loadedData.date);
     }

     [System.Serializable] //this attribute is required
     class DummyData //can also be a struct
     {
          public int number;
          public List<string> list;
          public DateTime date;
     }


     // private ------------------------------------------------------------------------------------
     static void _SaveData<T>(string path, T content)
     {
          if (log) Debug.Log("SaveData: Type = " + typeof(T) + " Path = " + path);

          try
          {
               Directory.CreateDirectory(defaultPath);

               using (var stream = File.Open(path, FileMode.OpenOrCreate))
               {
                    var bf = new BinaryFormatter(); //save in binary
                    bf.Serialize(stream, content);
               }
          }
          catch (Exception ex)
          {
               Debug.LogError("Save error: " + path + " Message:" + ex.Message);
          }
     }

     static T _LoadData<T>(string path) where T : class
     {
          if (log) Debug.Log("LoadData: Type = " + typeof(T) + " Path = " + path);

          if (!File.Exists(path))
          {
               Debug.LogError("Path not found: " + path);
               return null;
          }

          try
          {
               using (var stream = File.Open(path, FileMode.Open))
               {
                    var bf = new BinaryFormatter();
                    return (T)bf.Deserialize(stream);
               }
          }
          catch (Exception ex)
          {
               Debug.LogError("Load error: " + path + " Message:" + ex.Message);
               return null;
          }
     }



}
