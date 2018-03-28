using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexInfoPanelController : MonoBehaviour {
  public Text terrainText;
  public Text climateText;
  public Text biomeText;

  // Use this for initialization
  void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

  public void showHexInfo(Hex hex)
  {
    this.terrainText.text = "Terrain: " + this.getTerrainTag(hex.terrainType);
    this.climateText.text = "Climate: " + this.getClimateTag(hex.climateType);
    this.biomeText.text = "Biome: " + hex.biome.name;
  }

  public string getClimateTag(ClimateType climateType)
  {
    switch (climateType)
    {
      case ClimateType.COOL: return "cool";
      case ClimateType.WARM: return "warm";
      case ClimateType.TEMPERATE:
      default: return "temperate";
    }
  }

  public string getTerrainTag(TerrainType terrainType)
  {
    switch (terrainType)
    {
      case TerrainType.LOW: return "low";
      case TerrainType.NORMAL: return "normal";
      case TerrainType.ELEVATED: return "elevated";
      case TerrainType.HIGH:
      default: return "high";
    }
  }
}
