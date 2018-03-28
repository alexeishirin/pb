using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapData
{
  public static string BIOMES_PERSIST_PATH = "mapData/biomes";
  public static string BIOME_LENSES_PERSIST_PATH = "mapData/biomelenses";

  public List<Biome> biomes;
  public List<Biome> biomeLenses;

  public static MapData loadData(string applicationPersistentPath)
  {
    MapData mapData = new MapData();

    string biomesJson = DataLoader.loadFile(applicationPersistentPath, BIOMES_PERSIST_PATH);
    mapData.biomes = JsonHelper.FromJson<Biome>(biomesJson);
    string biomeLensesJson = DataLoader.loadFile(applicationPersistentPath, BIOME_LENSES_PERSIST_PATH);
    mapData.biomeLenses = JsonHelper.FromJson<Biome>(biomeLensesJson);

    return mapData;
  }

}
