using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class MapGenerationSettings
{
  public static string MAP_GENERATION_SETTINGS_PERSIST_PATH = "settings";

  public int mapHeight = 25;
  public int mapWidth = 25;
  public bool isHugeIsland = false;
  public int minRegionSize = 20;
  public int maxRegionSize = 30;
  public int terrainClustersPercent = 50;
  public int terrainAdjucencyPercentBoost = 4;
  public int middleClimateStripeMinHeightPercent = 40;
  public int middleClimateStripeMaxHeightPercent = 60;
  public int concentratedDirectionDecrease = 2;
  public int marginalClimateThreshold = 60;

  public int waterChance = 20;

  public static MapGenerationSettings loadSettings(string applicationPersistentPath)
  {
    string mapGenerationSettingsJson = DataLoader.loadFile(applicationPersistentPath, MAP_GENERATION_SETTINGS_PERSIST_PATH);
    return JsonUtility.FromJson<MapGenerationSettings>(mapGenerationSettingsJson);
  }

  public static void saveSettings(string applicationPersistentPath, MapGenerationSettings mapGenerationSettings)
  {
    Debug.Log("saving...");
    string settingsJson = JsonUtility.ToJson(mapGenerationSettings);
    File.WriteAllText(applicationPersistentPath + "/" + MAP_GENERATION_SETTINGS_PERSIST_PATH + ".json", settingsJson);
    Debug.Log("saved");
  }

  public override string ToString()
  {
    return JsonUtility.ToJson(this);
  }
}
