using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Skill 
{

  public string name;
  public string parent;
  public bool used;
  public bool passive;
  public string action;
  public int points;

  public override string ToString()
  {
    //return "Skill:" + JsonUtility.ToJson(this);
    string passiveOrActive = this.passive ? "passive" : "active";
    return "Skill: " + this.name + " (" + this.points + ") (" + passiveOrActive + ")";
  }

  public Skill clone()
  {
    return JsonUtility.FromJson<Skill>(JsonUtility.ToJson(this));
  }
}
