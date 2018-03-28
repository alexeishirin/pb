﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class JsonHelper
{
  public static List<T> FromJson<T>(string json)
  {
    Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
    return wrapper.items;
  }

  public static string ToJson<T>(List<T> array)
  {
    Wrapper<T> wrapper = new Wrapper<T>();
    wrapper.items = array;
    return JsonUtility.ToJson(wrapper);
  }

  public static string ToJson<T>(List<T> array, bool prettyPrint)
  {
    Wrapper<T> wrapper = new Wrapper<T>();
    wrapper.items = array;
    return JsonUtility.ToJson(wrapper, prettyPrint);
  }

  [Serializable]
  private class Wrapper<T>
  {
    public List<T> items;
  }
}
