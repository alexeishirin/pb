using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Party
{
  public List<Character> characters = new List<Character>();

  public override string ToString()
  {
    string partyString = "Party:" + JsonUtility.ToJson(this);
    partyString += "Characters:[";
    foreach(Character character in characters)
    {
      partyString += character.ToString();
    }
    partyString += "]";

    return partyString;
  }

}
