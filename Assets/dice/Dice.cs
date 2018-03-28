using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Dice
{
  public static string SUCCESS = "success";
  public static string FAILURE = "failure";
  public static string OPPORTUNITY = "opportunity";
  public static string THREAT = "threat";


  public string name;
  public bool used;
  public string type;
  public string[] sides;


  public string roll()
  {
    return this.sides[UnityEngine.Random.Range(0, this.sides.Length)];
  }

  public static List<string> getOutcome(List<Dice> dices)
  {
    List<string> outcomes = new List<string>();
    foreach(Dice dice in dices)
    {
      outcomes.AddRange(dice.roll().Split(' '));
    }

    int successCount = 0;
    int failuresCount = 0;
    int opportunityCount = 0;
    int threatCount = 0;
    foreach(string outcome in outcomes)
    {
      if (outcome == SUCCESS) successCount++;
      if (outcome == FAILURE) failuresCount++;
      if (outcome == OPPORTUNITY) opportunityCount++;
      if (outcome == THREAT) threatCount++;
    }
    outcomes.Clear();

    if (successCount > failuresCount)
    {
      outcomes.Add(SUCCESS);
    } else if (failuresCount > successCount)
    {
      outcomes.Add(FAILURE);
    }

    if (opportunityCount > threatCount)
    {
      outcomes.Add(OPPORTUNITY);
    }
    else if (threatCount > opportunityCount)
    {
      outcomes.Add(THREAT);
    }

    return outcomes;
  }

  public override string ToString()
  {
    return "Dice:" + JsonUtility.ToJson(this);
  }

}
