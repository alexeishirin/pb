using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Ability
{
  public string name;
  public string parentSkill;
  public bool passive;
  public int skillBonus;
  public string currentHexTags;
  public string targetHexTags;
  public string actionCharacterTags;
  public string targetCharacterTags;

  public override string ToString()
  {
    return "Ability:" + JsonUtility.ToJson(this);
  }
}
