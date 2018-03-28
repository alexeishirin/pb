using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
  public static Map generateMap(int mapSeed, MapGenerationSettings mapGenerationSettings, MapData mapData, string applicationPersistentDataPath, System.Random randomGenerator)
  {
    Map map = new Map();

    if (mapSeed == 0) {
      mapSeed = UnityEngine.Random.Range(0, 100);
    }
    Debug.Log(mapSeed);

    randomGenerator = new System.Random(mapSeed);

    for (int y = 0; y < mapGenerationSettings.mapHeight; y++)
    {
      for (int x = 0; x < mapGenerationSettings.mapWidth; x++)
      {
        int xOffset = y / 2;
        map.hexes.Add(generateHex(x + xOffset, y));
      }
    }

    generateRegions(map, mapGenerationSettings, randomGenerator);

    generateTerrain(map, randomGenerator, mapGenerationSettings);

    generateClimate(map, randomGenerator, mapGenerationSettings);

    spawnWater(map, randomGenerator);

    setRiverPoints(map, randomGenerator);

    generateBiomes(map, mapData, randomGenerator);
    
    return map;
  }

  public static void generateRegions(Map map, MapGenerationSettings mapGenerationSettings, System.Random randomGenerator)
  {
    Vector2 firstRegionCorePosition = findFirstRegionCorePosition(randomGenerator, mapGenerationSettings);
    generateRegion(firstRegionCorePosition, 1, "First region", randomGenerator, map, mapGenerationSettings);
    int regionIndex = 2;
    while (canGenerateRegion(map, mapGenerationSettings) && map.regions.Count < mapGenerationSettings.mapHeight * mapGenerationSettings.mapWidth / mapGenerationSettings.minRegionSize)
    {
      generateRegion(getClosestToCenterFreeHex(map, mapGenerationSettings, randomGenerator), regionIndex, "First region", randomGenerator, map, mapGenerationSettings);
      regionIndex++;
    }

    removeSmallRegions(map);
    for (int i = 0; i < 2; i++)
    {
      fillHoles(map, randomGenerator);
    }

    removeEmptyTiles(map);
  }

  public static string getClimateTag(ClimateType climateType)
  {
    switch (climateType)
    {
      case ClimateType.COOL: return "cool";
      case ClimateType.WARM: return "warm";
      case ClimateType.TEMPERATE:
      default: return "temperate";
    }
  }

  public static void generateClimate(Map map, System.Random randomGenerator, MapGenerationSettings mapGenerationSettings)
  {
    ClimateType middleClimateType = ClimateType.TEMPERATE;
    ClimateType upperClimateType = ClimateType.COOL;
    ClimateType bottomClimateType = ClimateType.WARM;

    int minY = 100000;
    int maxY = -100000;
    foreach (Hex hex in map.hexes)
    {
      if (hex.y > maxY)
      {
        maxY = hex.y;
      }

      if (hex.y < minY)
      {
        minY = hex.y;
      }
    }
    int actualMapHeight = maxY - minY;
    int middleClimateStripeHeight = actualMapHeight * randomGenerator.Next(mapGenerationSettings.middleClimateStripeMinHeightPercent, mapGenerationSettings.middleClimateStripeMaxHeightPercent) / 100;
    int minMiddleClimateY = minY + (actualMapHeight - middleClimateStripeHeight) / 2;
    int maxMiddleClimateY = minY + (actualMapHeight + middleClimateStripeHeight) / 2;
    foreach (Region region in map.regions)
    {
      int topHexCount = 0;
      int middleHexCount = 0;
      int bottomHexCount = 0;

      List<Hex> regionHexes = getRegionHexes(map, region.id);
      foreach (Hex hex in regionHexes)
      {
        if (hex.y < minMiddleClimateY)
        {
          bottomHexCount++;
        }
        else if (hex.y >= minMiddleClimateY && hex.y <= maxMiddleClimateY)
        {
          middleHexCount++;
        }
        else if (hex.y > maxMiddleClimateY)
        {
          topHexCount++;
        }
      }

      ClimateType regionClimateType = middleClimateType;
      if (topHexCount >= (topHexCount + middleHexCount + bottomHexCount) * mapGenerationSettings.marginalClimateThreshold / 100)
      {
        regionClimateType = upperClimateType;
      }
      else if (bottomHexCount >= (topHexCount + middleHexCount + bottomHexCount) * mapGenerationSettings.marginalClimateThreshold / 100)
      {
        regionClimateType = bottomClimateType;
      }

      region.climateType = regionClimateType;
      foreach (Hex hex in regionHexes)
      {
        hex.climateType = regionClimateType;
        hex.addTag(getClimateTag(regionClimateType));
      }
    }
  }

  public static void setRiverPoints(Map map, System.Random randomGenerator) {
    foreach (Hex hex in map.hexes) {
      //set river points as a fraction of hex size
      float randomX = randomGenerator.Next(81) * 0.01f - 0.4f;
      float randomY = randomGenerator.Next(81) * 0.01f - 0.4f;
      hex.riverPoint = new Vector2(randomX, randomY);
    }
  }

  public static void spawnWater(Map map, System.Random randomGenerator)
  {
    CriticalResource water = CriticalResource.WATER;
    int waterHexesCount = map.hexes.Count * water.totalPercent / 100;
    for (int i = 0; i < waterHexesCount; i++)
    {
      int climateRoll = randomGenerator.Next(100);
      ClimateType randomClimate = ClimateType.TEMPERATE;
      if (climateRoll < water.colderClimatePercent)
      {
        randomClimate = ClimateType.COOL;
      }
      else if (climateRoll >= water.colderClimatePercent + water.averageClimatePercent)
      {
        randomClimate = ClimateType.WARM;
      }

      List<Region> climateRegions = getRegionsByClimate(map, randomClimate);
      Region randomRegion = climateRegions[randomGenerator.Next(climateRegions.Count)];
      List<Hex> regionHexes = getRegionHexes(map, randomRegion.id);
      if (regionHexes.Count == 0)
      {
        continue;
      }
      List<Hex> possiblePlacementHexes = new List<Hex>();
      foreach (string priorityTag in water.priorityTags)
      {
        foreach (Hex hex in regionHexes)
        {
          if (hex.hasTag(priorityTag) && !hex.hasTag(water.name))
          {
            possiblePlacementHexes.Add(hex);
          }
        }
        if (possiblePlacementHexes.Count > 0)
        {
          break;
        }
      }

      if(possiblePlacementHexes.Count == 0)
      {
        continue;
      }

      Hex randomHex = possiblePlacementHexes[randomGenerator.Next(possiblePlacementHexes.Count)];
      randomHex.addTag(water.name);
      randomHex.removeTag(water.adjacentHexName);
      List<Vector2> waterHexNeighbours = HexMathHelper.getHexNeighbours(new Vector2(randomHex.x, randomHex.y), 1);
      foreach (Vector2 waterHexNeighbourCoords in waterHexNeighbours)
      {
        Hex neighbourHex = getHex(waterHexNeighbourCoords.x, waterHexNeighbourCoords.y, map);
        if (neighbourHex != null && !neighbourHex.hasTag(water.name) && !neighbourHex.hasTag(water.adjacentHexName))
        {
          neighbourHex.addTag(water.adjacentHexName);
        }
      }
    }
  }

  public static void generateBiomes(Map map, MapData mapData, System.Random randomGenerator)
  {
    foreach (Region region in map.regions)
    {
      List<Hex> regionHexes = getRegionHexes(map, region.id);
      shuffleList(regionHexes, randomGenerator);
      foreach (Hex hex in regionHexes)
      {
        List<Biome> fittingBiomes = new List<Biome>();
        foreach (Biome biome in mapData.biomes)
        {
          if (biome.requiredTerrain.Trim() != "" && !hex.hasAnyTag(biome.requiredTerrain))
          {
            continue;
          }

          if (biome.requiredClimate.Trim() != "" && !hex.hasAnyTag(biome.requiredClimate))
          {
            continue;
          }

          if (biome.requiredHexTags.Trim() != "" && !hex.hasAnyTag(biome.requiredHexTags))
          {
            continue;
          }

          if (biome.rejectingHexTags.Trim() != "" && hex.hasAnyTag(biome.rejectingHexTags))
          {
            continue;
          }

          if (biome.requiredAdjacentHexTags.Trim() != "" && !anyNeighbourHasTags(hex, biome.requiredAdjacentHexTags, map))
          {
            continue;
          }

          if (biome.rejectingAdjacentHexTags.Trim() != "" && anyNeighbourHasTags(hex, biome.rejectingAdjacentHexTags, map))
          {
            continue;
          }
          fittingBiomes.Add(biome);
        }
        if(fittingBiomes.Count == 0)
        {
          continue;
        }

        Biome basicBiome = fittingBiomes[randomGenerator.Next(fittingBiomes.Count)].clone();
        List<Biome> availableBiomeLenses = getBiomeLenses(mapData, basicBiome);
        bool gotFittingLens = false;
        if (availableBiomeLenses.Count > 0)
        {
          foreach (Biome biomeLense in availableBiomeLenses)
          {
            if (biomeLense.weightTags.Trim() != "" && hex.hasAnyTag(biomeLense.weightTags))
            {
              basicBiome.applyLens(biomeLense);
              gotFittingLens = true;
              break;
            }
          }
        }
        if (!gotFittingLens)
        {
          availableBiomeLenses.Add(basicBiome);
          Biome chosenBiome = availableBiomeLenses[randomGenerator.Next(availableBiomeLenses.Count)];
          if (chosenBiome != basicBiome)
          {
            basicBiome.applyLens(chosenBiome);
          }
        }

        hex.biome = basicBiome;
        hex.addTag(basicBiome.getBiomeTag());
      }
    }
  }

  public static void generateTerrain(Map map, System.Random randomGenerator, MapGenerationSettings mapGenerationSettings)
  {
    foreach (Region region in map.regions)
    {
      region.terrainProfile = TerrainProfile.TERRAIN_PROFILES[randomGenerator.Next(TerrainProfile.TERRAIN_PROFILES.Length)];
      List<Hex> regionHexes = getRegionHexes(map, region.id);
      int lowTerrainClustersCount = regionHexes.Count * mapGenerationSettings.terrainClustersPercent / 100 * region.terrainProfile.lowPercent / 100;
      int normalTerrainClustersCount = regionHexes.Count * mapGenerationSettings.terrainClustersPercent / 100 * region.terrainProfile.normalPercent / 100;
      int elevatedTerrainClustersCount = regionHexes.Count * mapGenerationSettings.terrainClustersPercent / 100 * region.terrainProfile.elevatedPercent / 100;
      int highTerrainClustersCount = regionHexes.Count * mapGenerationSettings.terrainClustersPercent / 100 * region.terrainProfile.highPercent / 100;

      shuffleList(regionHexes, randomGenerator);
      for (int lowTerrain = 0; lowTerrain < lowTerrainClustersCount; lowTerrain++)
      {
        if (regionHexes.Count > 0)
        {
          regionHexes[0].terrainType = TerrainType.LOW;
          regionHexes[0].addTag("low");
          regionHexes.RemoveAt(0);
        }
      }
      for (int normalTerrain = 0; normalTerrain < normalTerrainClustersCount; normalTerrain++)
      {
        if (regionHexes.Count > 0)
        {
          regionHexes[0].terrainType = TerrainType.NORMAL;
          regionHexes[0].addTag("normal");
          regionHexes.RemoveAt(0);
        }
      }
      for (int elevatedTerrain = 0; elevatedTerrain < elevatedTerrainClustersCount; elevatedTerrain++)
      {
        if (regionHexes.Count > 0)
        {
          regionHexes[0].terrainType = TerrainType.ELEVATED;
          regionHexes[0].addTag("elevated");
          regionHexes.RemoveAt(0);
        }
      }
      for (int highTerrain = 0; highTerrain < highTerrainClustersCount; highTerrain++)
      {
        if (regionHexes.Count > 0)
        {
          regionHexes[0].terrainType = TerrainType.HIGH;
          regionHexes[0].addTag("high");
          regionHexes.RemoveAt(0);
        }
      }
      foreach (Hex hex in regionHexes)
      {
        List<Vector2> hexNeighbours = HexMathHelper.getHexNeighbours(new Vector2(hex.x, hex.y), 1);
        int lowAdjacentHexCount = 0;
        int normalAdjacentHexCount = 0;
        int elevatedAdjacentHexCount = 0;
        int highAdjacentHexCount = 0;
        foreach (Vector2 hexCoordinate in hexNeighbours)
        {
          Hex neighbourHex = getHex(hexCoordinate.x, hexCoordinate.y, map);
          if (neighbourHex != null)
          {
            if (neighbourHex.terrainType == TerrainType.LOW)
            {
              lowAdjacentHexCount++;
            }
            else if (neighbourHex.terrainType == TerrainType.NORMAL)
            {
              normalAdjacentHexCount++;
            }
            else if (neighbourHex.terrainType == TerrainType.ELEVATED)
            {
              elevatedAdjacentHexCount++;
            }
            else if (neighbourHex.terrainType == TerrainType.HIGH)
            {
              highAdjacentHexCount++;
            }
          }
        }

        while (lowAdjacentHexCount > 0 && normalAdjacentHexCount > 0 && elevatedAdjacentHexCount > 0 && highAdjacentHexCount > 0)
        {
          lowAdjacentHexCount--;
          normalAdjacentHexCount--;
          elevatedAdjacentHexCount--;
          highAdjacentHexCount--;
        }

        int lowPercentForThisHex = region.terrainProfile.lowPercent + lowAdjacentHexCount * mapGenerationSettings.terrainAdjucencyPercentBoost;
        int normalPercentForThisHex = region.terrainProfile.normalPercent + normalAdjacentHexCount * mapGenerationSettings.terrainAdjucencyPercentBoost;
        int elevatedPercentForThisHex = region.terrainProfile.elevatedPercent + elevatedAdjacentHexCount * mapGenerationSettings.terrainAdjucencyPercentBoost;
        int highPercentForThisHex = region.terrainProfile.highPercent + highAdjacentHexCount * mapGenerationSettings.terrainAdjucencyPercentBoost;

        int adjacentTerrainClusters = lowAdjacentHexCount + normalAdjacentHexCount + elevatedAdjacentHexCount + highAdjacentHexCount;
        if (adjacentTerrainClusters > 0)
        {
          int numberOfDifferentAdjacentTerrains = 0;
          if (lowAdjacentHexCount > 0)
          {
            numberOfDifferentAdjacentTerrains++;
          }
          if (normalAdjacentHexCount > 0)
          {
            numberOfDifferentAdjacentTerrains++;
          }
          if (elevatedAdjacentHexCount > 0)
          {
            numberOfDifferentAdjacentTerrains++;
          }
          if (highAdjacentHexCount > 0)
          {
            numberOfDifferentAdjacentTerrains++;
          }

          int overPercent = adjacentTerrainClusters * mapGenerationSettings.terrainAdjucencyPercentBoost;
          int percentSubstract = overPercent / (4 - numberOfDifferentAdjacentTerrains);

          if (lowAdjacentHexCount == 0)
          {
            lowPercentForThisHex -= percentSubstract;
          }
          if (normalAdjacentHexCount == 0)
          {
            normalPercentForThisHex -= percentSubstract;
          }
          if (elevatedAdjacentHexCount == 0)
          {
            elevatedPercentForThisHex -= percentSubstract;
          }
          if (highAdjacentHexCount == 0)
          {
            highPercentForThisHex -= percentSubstract;
          }

          int roll = randomGenerator.Next(lowPercentForThisHex + normalPercentForThisHex + elevatedPercentForThisHex + highPercentForThisHex);
          if (roll < lowPercentForThisHex)
          {
            hex.terrainType = TerrainType.LOW;
            hex.addTag("low");
          }
          else if (roll < normalPercentForThisHex + lowPercentForThisHex)
          {
            hex.terrainType = TerrainType.NORMAL;
            hex.addTag("normal");
          }
          else if (roll < elevatedPercentForThisHex + normalPercentForThisHex + lowPercentForThisHex)
          {
            hex.terrainType = TerrainType.ELEVATED;
            hex.addTag("elevated");
          }
          else if (roll < highPercentForThisHex + elevatedPercentForThisHex + normalPercentForThisHex + lowPercentForThisHex)
          {
            hex.terrainType = TerrainType.HIGH;
            hex.addTag("high");
          }
        }
      }
    }
  }

  public static List<Hex> getRegionHexes(Map map, int regionId)
  {
    List<Hex> regionHexes = new List<Hex>();
    foreach (Hex hex in map.hexes)
    {
      if (hex.regionId == regionId)
      {
        regionHexes.Add(hex);
      }
    }

    return regionHexes;
  }

  public static void removeSmallRegions(Map map)
  {
    int[] regionSizes = new int[map.hexes.Count];
    foreach (Hex hex in map.hexes)
    {
      if (hex.regionId != 0)
      {
        regionSizes[hex.regionId - 1]++;
      }
    }

    for (int i = 0; i < regionSizes.Length; i++)
    {
      if (regionSizes[i] < 6)
      {
        foreach (Hex hex in map.hexes)
        {
          if (hex.regionId == i + 1)
          {
            hex.regionId = 0;
          }
        }
      }
    }
  }

  public static void removeEmptyTiles(Map map)
  {
    for (int i = map.hexes.Count - 1; i >= 0; i--)
    {
      Hex hex = map.hexes[i];
      if (hex.regionId == 0)
      {
        map.hexes.RemoveAt(i);
      }
    }
  }

  public static void fillHoles(Map map, System.Random randomGenerator)
  {
    foreach (Hex hex in map.hexes)
    {
      if (hex.regionId != 0)
      {
        continue;
      }

      List<Vector2> hexNeighbourCoordinates = HexMathHelper.getHexNeighbours(new Vector2(hex.x, hex.y), 1);
      int regionHexNeighboursCount = 0;
      foreach (Vector2 hexNeighbourCoordinate in hexNeighbourCoordinates)
      {
        Hex neighbourHex = getHex(hexNeighbourCoordinate.x, hexNeighbourCoordinate.y, map);
        if (neighbourHex != null && neighbourHex.regionId != 0)
        {
          regionHexNeighboursCount++;
        }
      }
      if (regionHexNeighboursCount >= 4)
      {
        shuffleList(hexNeighbourCoordinates, randomGenerator);
        foreach (Vector2 hexNeighbourCoordinate in hexNeighbourCoordinates)
        {
          Hex neighbourHex = getHex(hexNeighbourCoordinate.x, hexNeighbourCoordinate.y, map);
          if (neighbourHex != null && neighbourHex.regionId != 0)
          {
            hex.regionId = neighbourHex.regionId;
            break;
          }
        }
      }
    }
  }

  public static List<Biome> getBiomeLenses(MapData mapData, Biome biome)
  {
    List<Biome> biomeLenses = new List<Biome>();
    foreach (Biome biomeLense in mapData.biomeLenses)
    {
      if (biomeLense.parent == biome.name)
      {
        biomeLenses.Add(biomeLense);
      }
    }

    return biomeLenses;
  }

  public static bool anyNeighbourHasTags(Hex hex, string tags, Map map)
  {
    List<Vector2> hexNeighbours = HexMathHelper.getHexNeighbours(new Vector2(hex.x, hex.y), 1);
    foreach (Vector2 hexCoordinate in hexNeighbours)
    {
      Hex neighbourHex = getHex(hexCoordinate.x, hexCoordinate.y, map);
      if (neighbourHex != null && neighbourHex.hasAnyTag(tags))
      {
        return true;
      }
    }

    return false;
  }

  public static List<Region> getRegionsByClimate(Map map, ClimateType climateType)
  {
    List<Region> regionsWithClimate = new List<Region>();
    foreach (Region region in map.regions)
    {
      if (region.climateType == climateType)
      {
        regionsWithClimate.Add(region);
      }
    }

    return regionsWithClimate;
  }

  static Vector2 findFirstRegionCorePosition(System.Random randomGenerator, MapGenerationSettings mapGenerationSettings)
  {
    List<Vector2> possiblePositions = new List<Vector2>();
    //possiblePositions.Add(new Vector2(1 + 2, 2));
    //possiblePositions.Add(new Vector2(mapWidth - 1 - 2 + 1, 2));
    //possiblePositions.Add(new Vector2((mapHeight - 1 - 2) / 2 + 2, mapHeight - 1 - 2));
    //possiblePositions.Add(new Vector2(mapWidth - 1 - 2 + (mapHeight - 1 - 2) / 2, mapHeight - 1 - 2));
    possiblePositions.Add(mapCenter(mapGenerationSettings));

    return possiblePositions[randomGenerator.Next(possiblePositions.Count)];
  }

  public static Vector2 mapCenter(MapGenerationSettings mapGenerationSettings)
  {
    return new Vector2(mapGenerationSettings.mapWidth / 2 + mapGenerationSettings.mapHeight / 4, mapGenerationSettings.mapHeight / 2);
  }

  public static Vector2 getClosestToCenterFreeHex(Map map, MapGenerationSettings mapGenerationSettings, System.Random randomGenerator)
  {
    Vector2 mapCenter = MapGenerator.mapCenter(mapGenerationSettings);
    for (int radius = 1; radius < mapGenerationSettings.mapWidth / 2; radius++)
    {
      List<Vector2> circleHexes = HexMathHelper.getCircleAround(mapCenter, radius);
      shuffleList(circleHexes, randomGenerator);
      foreach (Vector2 hexPosition in circleHexes)
      {
        Hex hex = getHex((int)hexPosition.x, (int)hexPosition.y, map);
        if (hex != null && hex.regionId == 0)
        {
          return hexPosition;
        }
      }
    }

    return new Vector2(0, 0);
  }

  public static void shuffleList(List<Vector2> list, System.Random randomGenerator)
  {
    for (int i = list.Count; i > 1; i--)
    {
      int position = randomGenerator.Next(i);
      Vector2 temporary = list[i - 1];
      list[i - 1] = list[position];
      list[position] = temporary;
    }
  }

  public static void shuffleList(List<Hex> hexList, System.Random randomGenerator)
  {
    for (int i = hexList.Count; i > 1; i--)
    {
      int position = randomGenerator.Next(i);
      Hex temporary = hexList[i - 1];
      hexList[i - 1] = hexList[position];
      hexList[position] = temporary;
    }
  }

  static bool canGenerateRegion(Map map, MapGenerationSettings mapGenerationSettings)
  {
    return getOccupiedHexCount(map) < (mapGenerationSettings.mapWidth * mapGenerationSettings.mapHeight - mapGenerationSettings.maxRegionSize) && getFreeHexCount(map) > mapGenerationSettings.minRegionSize;
  }

  static void generateRegion(Vector2 corePosition, int id, string name, System.Random randomGenerator, Map map, MapGenerationSettings mapGenerationSettings)
  {
    Color32 regionColor = new Color32((byte)randomGenerator.Next(170), (byte)randomGenerator.Next(170), (byte)randomGenerator.Next(170), 255);
    Region region = new Region(id, name, regionColor);
    map.regions.Add(region);
    region.growthType = getRandomGrowthType(randomGenerator);
    generateConcentratedRegion(region, corePosition, map, randomGenerator, mapGenerationSettings);
  }

  static void generateConcentratedRegion(Region region, Vector2 corePosition, Map map, System.Random randomGenerator, MapGenerationSettings mapGenerationSettings)
  {
    Hex coreHex = getHex((int)corePosition.x, (int)corePosition.y, map);
    if (coreHex == null)
    {
      coreHex = generateHex((int)corePosition.x, (int)corePosition.y);
    }
    coreHex.regionId = region.id;

    int regionSize = randomGenerator.Next(mapGenerationSettings.minRegionSize, mapGenerationSettings.maxRegionSize);
    Vector2 currentPosition = corePosition;
    int currentSize = 1;
    while (currentSize <= regionSize)
    {
      List<Vector2> possibleDirections = MapGenerator.possibleDirections(map, currentPosition);
      if (possibleDirections.Count == 0)
      {
        break;
      }
      int[] weightsArray = new int[possibleDirections.Count];
      for (int i = 0; i < possibleDirections.Count; i++)
      {
        int weight = 1;
        int neighboursFromSameRegion = 0;
        Vector2 neighbour = possibleDirections[i];
        List<Vector2> neighbourCoordinates = HexMathHelper.getHexNeighbours(new Vector2(neighbour.x, neighbour.y), 1);
        foreach (Vector2 neighbourCoordinate in neighbourCoordinates)
        {
          Hex hex = getHex((int)neighbourCoordinate.x, (int)neighbourCoordinate.y, map);
          if (hex != null && hex.regionId == region.id)
          {
            neighboursFromSameRegion++;
          }
        }
        //there's always one neighbour from the region
        weight = weight * (int)Mathf.Pow(10, (neighboursFromSameRegion - 1));

        //discourage going out of bounds
        if (getHex((int)neighbour.x, (int)neighbour.y, map) == null)
        {
          weight = neighboursFromSameRegion;
        }
        weightsArray[i] = weight;
      }

      int weightSum = arraySum(weightsArray);
      int roll = randomGenerator.Next(weightSum);

      for (int weightIndex = 0; weightIndex < weightsArray.Length; weightIndex++)
      {
        int currentWeightSum = 0;
        for (int weightSumIndex = 0; weightSumIndex <= weightIndex; weightSumIndex++)
        {
          currentWeightSum += weightsArray[weightSumIndex];
        }
        if (roll < currentWeightSum)
        {
          currentPosition = possibleDirections[weightIndex];
          break;
        }
      }

      Hex nextHex = getHex((int)currentPosition.x, (int)currentPosition.y, map);
      if (nextHex == null)
      {
        nextHex = generateHex((int)currentPosition.x, (int)currentPosition.y);
        map.hexes.Add(nextHex);
      }

      nextHex.regionId = region.id;
      currentSize++;
    }
  }

  public static List<Vector2> possibleDirections(Map map, Vector2 currentPosition)
  {
    List<Vector2> possibleDirections = new List<Vector2>();
    List<Vector2> neighbourCoordinates = HexMathHelper.getHexNeighbours(currentPosition, 1);
    foreach (Vector2 neighbourCoordinate in neighbourCoordinates)
    {
      Hex hex = getHex((int)neighbourCoordinate.x, (int)neighbourCoordinate.y, map);
      if (hex == null || hex.regionId == 0)
      {
        possibleDirections.Add(neighbourCoordinate);
      }
    }

    return possibleDirections;
  }

  public static int getFreeHexCount(Map map)
  {
    int freeHexCount = 0;
    foreach (Hex hex in map.hexes)
    {
      if (hex.regionId == 0)
      {
        freeHexCount++;
      }
    }

    return freeHexCount;
  }

  public static int getOccupiedHexCount(Map map)
  {
    int occupiedHexCount = 0;
    foreach (Hex hex in map.hexes)
    {
      if (hex.regionId != 0)
      {
        occupiedHexCount++;
      }
    }

    return occupiedHexCount;
  }

  public static int arraySum(int[] weightsArray)
  {
    int sum = 0;
    foreach (int weight in weightsArray)
    {
      sum += weight;
    }

    return sum;
  }

  public static RegionGrowthType getRandomGrowthType(System.Random randomGenerator)
  {
    return (RegionGrowthType)randomGenerator.Next(System.Enum.GetValues(typeof(RegionGrowthType)).Length);
  }

  public static TerrainType getRandomTerrainType(System.Random randomGenerator)
  {
    return (TerrainType)randomGenerator.Next(System.Enum.GetValues(typeof(TerrainType)).Length);
  }

  public static Hex generateHex(int x, int y)
  {
    Hex hex = new Hex();
    hex.x = x;
    hex.y = y;

    return hex;
  }

  public Region getRegionById(int id, Map map)
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

  public static Hex getHex(int x, int y, Map map)
  {
    foreach (Hex hex in map.hexes)
    {
      if (hex.x == x && hex.y == y)
      {
        return hex;
      }
    }

    return null;
  }

  public static Hex getHex(float x, float y, Map map)
  {
    foreach (Hex hex in map.hexes)
    {
      if (hex.x == (int)x && hex.y == (int)y)
      {
        return hex;
      }
    }

    return null;
  }
}
