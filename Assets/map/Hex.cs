using UnityEngine;

public class Hex 
{
  public TerrainType terrainType;
  public ClimateType climateType;
  public Biome biome;
  public int x;
  public int y;
  public int regionId = 0;
  public string tags = "";
  public Vector2 riverPoint;

  public Hex(int x, int y, TerrainType terrainType, Vector2 riverPoint)
  {
    this.x = x;
    this.y = y;
    this.terrainType = terrainType;
    this.regionId = 0;
  }

  public Hex()
  {

  }

  public override string ToString()
  {
    return "Hex:" + JsonUtility.ToJson(this);
  }

  public bool hasTag(string tag)
  {
    return this.tags.IndexOf(" " + tag + " ") != -1;
  }

  public bool hasAnyTag(string tags)
  {
    string[] tagArray = tags.Split(' ');
    foreach(string tag in tagArray)
    {
      if(tag.Trim() != "" && this.hasTag(tag))
      {
        return true;
      }
    }

    return false;
  }

  public void addTag(string tag)
  {
    this.tags += " " + tag + " ";
  }
  public void removeTag(string tag)
  {
    this.tags = this.tags.Replace(" " + tag + " ", "");
  }
}