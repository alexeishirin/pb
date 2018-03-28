using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Biome
{
  public string name;
  public bool active;
  public string caption;
  public string[] descriptions;
  public string[] actionDescriptions;
  public int criticalResourceMinimumAmount;
  public int criticalResourceMaximumAmount;
  public string requiredTerrain;
  public string requiredClimate;
  public string requiredHexTags;
  public string rejectingHexTags;
  public string requiredAdjacentHexTags;
  public string rejectingAdjacentHexTags;
  public string tags;
  public string poi;
  public string overlayObjects;

  //Lens
  public string parent;
  public string weightTags;
  public string lensTags;
  public string tagsToDelete;


  public string getBiomeTag()
  {
    return this.name.Replace(' ', '_');
  }


  public override string ToString()
  {
    return "Biome:" + JsonUtility.ToJson(this);
  }

  public void applyLens(Biome lens)
  {
    this.parent = this.name;
    this.name = lens.name;
    this.active = lens.active;
    this.caption = lens.caption;
    this.descriptions = lens.descriptions;
    this.actionDescriptions = lens.actionDescriptions;
    this.poi = lens.poi.Trim() != "" ? lens.poi : this.poi;
    this.overlayObjects = lens.overlayObjects.Trim() != "" ? lens.overlayObjects: this.overlayObjects;
    this.lensTags = lens.lensTags;
    this.tagsToDelete = lens.tagsToDelete;
  }

  public Biome clone()
  {
    return JsonUtility.FromJson<Biome>(JsonUtility.ToJson(this));
  }
}
