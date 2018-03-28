using UnityEngine;
using System.Collections;

public class CriticalResource
{
  public static string[] WATER_PRIORITY_TAGS = { "low", "normal", "elevated", "high" };
  public static CriticalResource WATER = new CriticalResource("CR", "CRadj", 20, 30, 40, 30, WATER_PRIORITY_TAGS);

  public string name;
  public string adjacentHexName;
  public int totalPercent;

  public int colderClimatePercent;
  public int averageClimatePercent;
  public int warmerClimatePercent;

  public string[] priorityTags;

  public CriticalResource(string name, string adjacentHexName, int totalPercent, int colderClimatePercent, int averageClimatePercent, int warmerClimatePercent, string[] priorityTags)
  {
    this.name = name;
    this.adjacentHexName = adjacentHexName;
    this.totalPercent = totalPercent;
    this.colderClimatePercent = colderClimatePercent;
    this.averageClimatePercent = averageClimatePercent;
    this.warmerClimatePercent = warmerClimatePercent;
    this.priorityTags = priorityTags;
  }

}
