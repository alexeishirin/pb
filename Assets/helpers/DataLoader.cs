using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public class DataLoader
{

  public static string DATA_PATH = "Assets";

  public static string loadFile(string applicationPersistentPath, string path)
  {
    Debug.Log("loading...");
    string filePersistentPath = applicationPersistentPath + "/" + path + ".json";
    if (File.Exists(filePersistentPath))
    {
      return File.ReadAllText(filePersistentPath);
    }

    //string json = File.ReadAllText(DATA_PATH + path);
    string json = ((TextAsset)Resources.Load(path)).text;

    // Write that JSON string to the specified file.
    FileInfo file = new FileInfo(filePersistentPath);
    file.Directory.Create();
    File.WriteAllText(file.FullName, json);

    return json;
  }
}