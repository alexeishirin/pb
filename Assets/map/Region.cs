using UnityEngine;

public class Region {
  public int id;
  public string name;
  public RegionGrowthType growthType;
  public ClimateType climateType;
  public TerrainProfile terrainProfile;
  public Color32 color;
  public string tags = "";
  public Region(int id, string name, Color32 color)
  {
    this.id = id;
    this.name = name;
    this.color = color;
  }

  public override string ToString()
  {
    return "Region:" + JsonUtility.ToJson(this);
  }

  public bool hasTag(string tag)
  {
    return this.tags.IndexOf(tag) != -1;
  }

  public void addTag(string tag)
  {
    this.tags += " " + tag;
  }
}