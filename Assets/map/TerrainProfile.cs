using UnityEngine;
using System.Collections;

public class TerrainProfile
{
  public static TerrainProfile GENERIC = new TerrainProfile(20, 55, 20, 5);
  public static TerrainProfile LOWLANDS = new TerrainProfile(50, 35, 15, 0);
  public static TerrainProfile ROUGH = new TerrainProfile(20, 20, 40, 20);

  public static TerrainProfile[] TERRAIN_PROFILES = { GENERIC, LOWLANDS, ROUGH };

  public int lowPercent;
  public int normalPercent;
  public int elevatedPercent;
  public int highPercent;

  public TerrainProfile(int lowPercent, int normalPercent, int elevatedPercent, int highPercent)
  {
    this.lowPercent = lowPercent;
    this.normalPercent = normalPercent;
    this.elevatedPercent = elevatedPercent;
    this.highPercent = highPercent;
  }
}
