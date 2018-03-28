using UnityEngine;
using System;

[Serializable]
public class MovementPenalty
{
  public string hexTags;
  public int penalty;

  public int calculatePenalty(Hex hex)
  {
    int penalty = 0;
    string[] penaltyTags = hexTags.Split(' ');
    foreach(string penaltyTag in penaltyTags)
    {
      if(hex.hasTag(penaltyTag))
      {
        penalty += this.penalty;
      }
    }

    return penalty;
  }

  public override string ToString()
  {
    return "MovementPenalty:" + JsonUtility.ToJson(this);
  }
}