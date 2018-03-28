using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Character
{

  public List<Skill> skills = new List<Skill>();
  public List<Ability> abilities = new List<Ability>();

  public override string ToString()
  {
    string characterString = "\nCharacter:";// + JsonUtility.ToJson(this);
    characterString += "\nSkills:[";
    foreach(Skill skill in skills)
    {
      characterString += "\n" + skill.ToString();
    }
    characterString += "]";
    characterString += "\nAbilities:[";
    foreach(Ability ability in abilities)
    {
      characterString += "\n" + ability.ToString();
    }
    characterString += "]";

    return characterString;
  }
}
