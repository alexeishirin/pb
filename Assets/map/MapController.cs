using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapController : MonoBehaviour {

  public MapGenerationSettings mapGenerationSettings;
  public MapData mapData;

  public System.Random randomGenerator;

  public GameObject hexPrefab;

  public int specifiedMapSeed = 0;

  public Map map;

	// Use this for initialization
	void Start () {
    //MapGenerator.generateMap();
    //drawMap();
  }
	
	// Update is called once per frame
	void Update () {
		
	}

  public void regenerateMap(Map newMap)
  {
    deleteHexObjects();
    this.map = newMap;
    drawMap();
  }

  public void deleteHexObjects()
  {
    GameObject[] hexObjects = GameObject.FindGameObjectsWithTag("Hex");
    foreach(GameObject hex in hexObjects)
    {
      Destroy(hex);
    }
  }

  public Region getRegionById(int id)
  {
    foreach (Region region in map.regions)
    {
      if (region.id == id)
      {
        return region;
      }
    }

    return null;
  }

  public void clearMap()
  {
    this.map = null;
  }

  public void setMapSeed(int newSeed)
  {
    this.specifiedMapSeed = newSeed;
  }

  public void drawMap()
  {
    foreach(Hex hex in map.hexes)
    {
      drawHex(hex);
    }
  }

  void drawHex(Hex hex)
  {
    GameObject hexObject = (GameObject)Instantiate(hexPrefab, Vector3.zero, Quaternion.identity);
    hexObject.transform.position = HexMathHelper.hexToWorldCoords(new Vector2(hex.x, hex.y), this.getHexSize());
    HexController hexController = hexObject.GetComponent<HexController>();
    hexController.hexText.text = hex.x + ":" + hex.y;
    hexController.hex = hex;
    if (hex.regionId != 0) {
      hexObject.GetComponent<SpriteRenderer>().color = this.getRegionById(hex.regionId).color;
    }
    //hexObject.GetComponent<SpriteRenderer>().color = getColorByTerrainType(hex.terrainType);
  }

  public void paintHexesByTerrain()
  {
    foreach (Hex hex in map.hexes)
    {
      GameObject hexObject = this.findHexObject(new Vector2(hex.x, hex.y));
      if (hexObject != null)
      {
        hexObject.GetComponent<SpriteRenderer>().color = getColorByTerrainType(hex.terrainType);
      } 
    }
  }

  public void paintHexesByClimate()
  {
    foreach (Hex hex in map.hexes)
    {
      GameObject hexObject = this.findHexObject(new Vector2(hex.x, hex.y));
      if (hexObject != null)
      {
        hexObject.GetComponent<SpriteRenderer>().color = getColorByClimateType(hex.climateType);
      }
    }
  }

  public void paintHexesByWater()
  {
    foreach (Hex hex in map.hexes)
    {
      GameObject hexObject = this.findHexObject(new Vector2(hex.x, hex.y));
      if (hexObject != null)
      {
        hexObject.GetComponent<SpriteRenderer>().color = getColorByHexWater(hex);
      }
    }
  }

  public void paintHexesByRegions()
  {
    foreach (Hex hex in map.hexes)
    {
      GameObject hexObject = this.findHexObject(new Vector2(hex.x, hex.y));
      if (hexObject != null && hex.regionId != 0)
      {
        hexObject.GetComponent<SpriteRenderer>().color = this.getRegionById(hex.regionId).color;
      }
    }
  }

  public GameObject findHexObject(Vector2 coordinates)
  {
    Vector2 hexObjectWorldCoordinates = HexMathHelper.hexToWorldCoords(coordinates, this.getHexSize());
    foreach (GameObject hexObject in GameObject.FindGameObjectsWithTag("Hex"))
    {
      Vector2 hexObject2dCoordinates = new Vector2(hexObject.transform.position.x, hexObject.transform.position.y);
      if (hexObjectWorldCoordinates == hexObject2dCoordinates)
      {
        return hexObject;
      }
    }

    return null;
  }

  public Color32 getColorByTerrainType(TerrainType terrainType)
  {
    switch(terrainType)
    {
      case TerrainType.HIGH: return new Color32(255, 20, 20, 255);
      case TerrainType.ELEVATED: return new Color32(255, 110, 110, 255);
      case TerrainType.NORMAL: return new Color32(255, 200, 200, 255);
      case TerrainType.LOW:
      default: return new Color32(255, 255, 255, 255);
    }
  }

  public Color32 getColorByClimateType(ClimateType climateType)
  {
    switch (climateType)
    {
      case ClimateType.COOL: return Color.blue;
      case ClimateType.WARM: return Color.red;
      case ClimateType.TEMPERATE:
      default: return Color.green;
    }
  }

  public Color32 getColorByHexWater(Hex hex)
  {
    if(hex.hasTag(CriticalResource.WATER.name))
    {
      return Color.blue;
    }

    if (hex.hasTag(CriticalResource.WATER.adjacentHexName))
    {
      return new Color32(200, 200, 255, 255);
    }


    return Color.white;
  }

  public float getHexSize()
  {
    GameObject hex = (GameObject)Instantiate(hexPrefab, Vector3.zero, Quaternion.identity, this.transform);
    float hexSize = hex.GetComponent<Renderer>().bounds.size.x;
    Destroy(hex);

    return hexSize;
  }

  public static MapController getInstance()
  {
    return GameObject.FindGameObjectWithTag("Map").GetComponent<MapController>();
  }
}
