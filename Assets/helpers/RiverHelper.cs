using UnityEngine;
using System.Collections.Generic;

public class RiverHelper {

  public static int riverOriginsCount = 20;
  public static int maxRiverLength = 30;
  public static int minRiverLength = 3;
  public static string FRESH_LAKE_TAG = "Fresh_lake";
  public static string ELEVATED_TERRAIN_TAG = "elevated";
  public static string NORMAL_TERRAIN_TAG = "normal";
  public static string LOW_TERRAIN_TAG = "low";
  public static string WATER_TAG = CriticalResource.WATER.name;
  public static string WATER_ADJACENT_TAG = CriticalResource.WATER.adjacentHexName;


  public static List<List<Vector2>> generateRivers(Map map) {
    List<List<Vector2>> rivers = new List<List<Vector2>>();

    List<Hex> riverOrigins = new List<Hex>();

    List<Hex> lakeHexes = map.hexes.FindAll(hex => hex.hasTag(FRESH_LAKE_TAG));
    riverOrigins.AddRange(lakeHexes);

    List<Hex> normalHeightWaterHexes = map.hexes.FindAll(hex => hex.hasTag(NORMAL_TERRAIN_TAG) && (hex.hasTag(WATER_TAG) || hex.hasTag(WATER_ADJACENT_TAG)));
    Debug.Log(normalHeightWaterHexes.Count);

    riverOrigins.AddRange(normalHeightWaterHexes);
    /*int minDistance = HexMathHelper.hexDistance(normalHeightWaterHexes[0].getPosition(), mapCenter);
    Hex nextOrigin = normalHeightWaterHexes[0];
    foreach (Hex normalHeightHex in normalHeightWaterHexes) {
      int distance = HexMathHelper.hexDistance(normalHeightHex.getPosition(), mapCenter);
      foreach (Hex riverOrigin in riverOrigins) {
        distance += HexMathHelper.hexDistance(normalHeightHex.getPosition(), riverOrigin.getPosition());
      }

      if (distance <= minDistance) {
        minDistance = distance;
        nextOrigin = normalHeightHex;
      }
    }

    if (nextOrigin != null) {
      riverOrigins.Add(nextOrigin);
      Debug.Log(nextOrigin.getPosition());

      Hex leftTopHex = map.getHex(nextOrigin.getPosition() + new Vector2(-5, 5));
      if (leftTopHex != null) {
        Debug.Log(leftTopHex.getPosition());
        riverOrigins.Add(leftTopHex);
      }

      Hex rightTopHex = map.getHex(nextOrigin.getPosition() + new Vector2(5, 5));
      if (rightTopHex != null) {
        Debug.Log(rightTopHex.getPosition());
        riverOrigins.Add(rightTopHex);
      }

      Hex leftBottomHex = map.getHex(nextOrigin.getPosition() + new Vector2(-5, -5));
      if (leftBottomHex != null) {
        Debug.Log(leftBottomHex.getPosition());
        riverOrigins.Add(leftBottomHex);
      }

      Hex rightbottomHex = map.getHex(nextOrigin.getPosition() + new Vector2(5, -5));
      if (rightbottomHex != null) {
        Debug.Log(rightbottomHex.getPosition());
        riverOrigins.Add(rightbottomHex);
      }
    }
    */

    Debug.Log(riverOrigins.Count);
    while (riverOrigins.Count > 0) {
      Hex riverOrigin = riverOrigins[0];
      List<Vector2> river = new List<Vector2>();
      river.Add(riverOrigin.getPosition());
      Hex currentHex = riverOrigin;
      Hex previousHex = null;
      List<Hex> riverHexes = new List<Hex>();
      riverHexes.Add(riverOrigin);

      for (int i = 0; i < maxRiverLength; i++) {
        List<Vector2> neighbourCoordinates = HexMathHelper.getHexNeighbours(currentHex.getPosition(), 1);

        //don't bend backwards
        if (river.Count > 1) {
          neighbourCoordinates = neighbourCoordinates.FindAll(coordinate =>
            HexMathHelper.hexDistance(coordinate, previousHex.getPosition()) > 1);
        }

        shuffleList(neighbourCoordinates);

        bool hasOceanNeigbours = false;
        foreach (Vector2 neighbourCoordinate in neighbourCoordinates) {
          if (map.hexes.Find(hex => hex.getPosition() == neighbourCoordinate) == null) {
            //we've reached ocean
            river.Add(neighbourCoordinate);
            hasOceanNeigbours = true;
            break;
          }
        }
        if (hasOceanNeigbours) {
          break;
        }

        List<Hex> possibleNextHexes = map.hexes.FindAll(hex => neighbourCoordinates.Contains(hex.getPosition()));
        if (previousHex != null && rivers.Exists(previousRiver => previousRiver.Contains(previousHex.getPosition()))) {
          possibleNextHexes = possibleNextHexes.FindAll(possibleNextHex => !rivers.Exists(previousRiver => previousRiver.Contains(possibleNextHex.getPosition())));
        }

        shuffleHexList(possibleNextHexes);
        Hex nextHex = null;// possibleNextHexes.Find(hex => hex.hasTag(FRESH_LAKE_TAG));
        if (nextHex == null && possibleNextHexes.Count != 0) {
          //nextHex = possibleNextHexes[Random.Range(0, possibleNextHexes.Count)];
          string terrainTag = currentHex.hasTag(ELEVATED_TERRAIN_TAG) ?
            ELEVATED_TERRAIN_TAG : currentHex.hasTag(NORMAL_TERRAIN_TAG) ? NORMAL_TERRAIN_TAG : LOW_TERRAIN_TAG;
          if (terrainTag == ELEVATED_TERRAIN_TAG) {
            nextHex = possibleNextHexes.Find(hex => hex.hasTag(ELEVATED_TERRAIN_TAG));
            if (nextHex == null) {
              nextHex = possibleNextHexes.Find(hex => hex.hasTag(NORMAL_TERRAIN_TAG));
            }

            if (nextHex == null) {
              nextHex = possibleNextHexes.Find(hex => hex.hasTag(LOW_TERRAIN_TAG));
            }
          } else if (terrainTag == NORMAL_TERRAIN_TAG) {
            nextHex = possibleNextHexes.Find(hex => hex.hasTag(NORMAL_TERRAIN_TAG));
            if (nextHex == null) {
              nextHex = possibleNextHexes.Find(hex => hex.hasTag(LOW_TERRAIN_TAG));
            }

            if (nextHex == null) {
              nextHex = possibleNextHexes.Find(hex => hex.hasTag(ELEVATED_TERRAIN_TAG));
            }
          } else if (terrainTag == LOW_TERRAIN_TAG) {
            nextHex = possibleNextHexes.Find(hex => hex.hasTag(LOW_TERRAIN_TAG));
            if (nextHex == null) {
              nextHex = possibleNextHexes.Find(hex => hex.hasTag(NORMAL_TERRAIN_TAG));
            }

            if (nextHex == null) {
              nextHex = possibleNextHexes.Find(hex => hex.hasTag(ELEVATED_TERRAIN_TAG));
            }
          }
        }

        //no suitable followups, give up on this river
        if (nextHex == null) {
          break;
        }

        river.Add(nextHex.getPosition());
        riverHexes.Add(nextHex);
        if (rivers.Exists(previousRiver => previousRiver.Contains(nextHex.getPosition()))) {
          break;
        }
        previousHex = currentHex;
        currentHex = nextHex;
      }
      if (river.Count >= minRiverLength) {
        rivers.Add(river);
        foreach (Hex hex in riverHexes) {
          riverOrigins.Remove(hex);
        }
      }
      if (rivers.Count >= riverOriginsCount) {
        break;
      }
      riverOrigins.Remove(riverOrigin);
    }
    Debug.Log(rivers.Count);
    return rivers;
  }

  public static List<List<Vector2>> getRiverPoints(List<List<Vector2>> rivers, float riverDensity, float hexSize, Map map) {
    float perlinNoiseX = 15.004f;
    float perlinNoiseStep = 0.1f;
    List<List<Vector2>> allRriversPoints = new List<List<Vector2>>();
    float riverThickness = riverDensity / 4;

    foreach (List<Vector2> river in rivers) {
      List<Vector2> riverShorePoints = new List<Vector2>();
      List<Vector2> riverPoints = new List<Vector2>();
      for (int i = 0; i < river.Count - 1; i++) {
        Vector2 firstRiverPoint = getHexRiverPoint(river[i], hexSize, map);
        Vector2 secondRiverPoint = getHexRiverPoint(river[i + 1], hexSize, map);
        Vector2 direction = secondRiverPoint - firstRiverPoint;
        Vector2 normal = new Vector2(-direction.y, direction.x).normalized;
        int pointsNumber = Mathf.CeilToInt(direction.magnitude / riverDensity);

        for (int pointIndex = 0; pointIndex < pointsNumber; pointIndex++) {
          Vector2 derivativeDirection;
          float t = pointIndex * 1.0f / pointsNumber;
          Vector2 newPoint = Vector2.Lerp(firstRiverPoint, secondRiverPoint, t);
          float scaleShiftFactor = pointIndex <= pointsNumber / 2 ? pointIndex * 2.0f / pointsNumber : (pointsNumber - pointIndex) * 2.0f / pointsNumber;
          newPoint = Vector2.Lerp(firstRiverPoint, secondRiverPoint, t);
          float shift = Mathf.PerlinNoise(perlinNoiseX, riverPoints.Count * perlinNoiseStep) * 0.4f - 0.2f;
          newPoint += normal * shift * scaleShiftFactor;

          if (i != 0 || pointIndex != 0) {
            if (pointIndex == 0) {
              float nextPointT = (pointIndex + 1) * 1.0f / pointsNumber;
              Vector2 nextPoint = Vector2.Lerp(firstRiverPoint, secondRiverPoint, nextPointT);
              float nextScaleShiftFactor = pointIndex + 1 <= pointsNumber / 2 ? (pointIndex + 1) * 2.0f / pointsNumber : (pointsNumber - pointIndex - 1) * 2.0f / pointsNumber;
              float nextPointShift = Mathf.PerlinNoise(perlinNoiseX, (riverPoints.Count + 1) * perlinNoiseStep) * 0.4f - 0.2f;
              nextPoint += normal * nextPointShift * nextScaleShiftFactor;

              Vector2 previousPoint = riverPoints[riverPoints.Count - 1];

              float derivativeT = 0.5f;
              derivativeDirection = 2f * (1f - derivativeT) * (newPoint - previousPoint) + 2f * derivativeT * (nextPoint - newPoint);

              newPoint = Vector2.Lerp(Vector2.Lerp(previousPoint, newPoint, 0.5f), Vector2.Lerp(newPoint, nextPoint, 0.5f), 0.5f);
            } else {
              Vector2 previousPoint = riverPoints[riverPoints.Count - 1];
              derivativeDirection = newPoint - previousPoint;
            }

            Vector3 perpendicular = new Vector3(-derivativeDirection.y, derivativeDirection.x).normalized;
            Vector3 actualNewPoint = new Vector3(newPoint.x, newPoint.y, 0);
            Vector3 firstShorePoint = actualNewPoint + perpendicular * riverThickness;
            Hex firstPointHex = map.getHex(HexMathHelper.worldToHexCoords(firstShorePoint, hexSize));

            Vector3 secondShorePoint = actualNewPoint - perpendicular * riverThickness;
            Hex secondPointHex = map.getHex(HexMathHelper.worldToHexCoords(secondShorePoint, hexSize));

            riverShorePoints.Add(firstShorePoint);
            riverShorePoints.Add(secondShorePoint);
          }

          Hex newPointHex = map.getHex(HexMathHelper.worldToHexCoords(newPoint, hexSize));
          riverPoints.Add(new Vector2(newPoint.x, newPoint.y));
        }
      }
      Vector3 veryFirstShorePoint = riverShorePoints[0];
      Vector3 verySecondShorePoint = riverShorePoints[1];
      Vector3 firstPointShift = new Vector2(riverPoints[0].x, riverPoints[0].y) - new Vector2(riverPoints[1].x, riverPoints[1].y);
      Vector3 missingFirstPoint = verySecondShorePoint + firstPointShift;
      riverShorePoints.Insert(0, missingFirstPoint);

      Vector3 missingSecondPoint = veryFirstShorePoint + firstPointShift;
      riverShorePoints.Insert(0, missingSecondPoint);


      riverPoints.AddRange(riverShorePoints);
      //allRriversPoints.AddRange(riverPoints);
      allRriversPoints.Add(riverShorePoints);
    }   
 
    return allRriversPoints;
  }

  public static List<Vector2> getRiverContour(List<Vector2> points) {
    List<Vector2> riverContour = new List<Vector2>();
    List<Vector2> arroundRiverBounds = new List<Vector2>();
    for (int i = 0; i <= points.Count - 2; i += 2) {
      Vector2 substraction = points[i + 1] - points[i];
      arroundRiverBounds.Add(points[i] - substraction);
      arroundRiverBounds.Add(points[i + 1] + substraction);
    }
    for (int i = 0; i <= arroundRiverBounds.Count - 2; i += 2) {
      riverContour.Add(arroundRiverBounds[i]);
    }

    for (int i = arroundRiverBounds.Count - 1; i >= 1; i -= 2) {
      riverContour.Add(arroundRiverBounds[i]);
    }

    return riverContour;
  }

  public static List<Vector3> getRiversMesh(List<List<Vector2>> rivers, float riverDensity, float hexSize, float heightSeed,
    Texture2D mountainHeightmap, Texture2D elevatedHeightmap, Map map) {
    Debug.Log(rivers.Count);
    float perlinNoiseX = 15.004f;
    float perlinNoiseStep = 0.1f;
    List<Vector3> allRriversPoints = new List<Vector3>();
    float riverThickness = riverDensity;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Color32> colours = new List<Color32>();

    foreach (List<Vector2> river in rivers) {
      Debug.Log(river[0]);
      Debug.Log(river.Count);
      List<Vector3> riverShorePoints = new List<Vector3>();
      List<Vector3> riverPoints = new List<Vector3>();
      for (int i = 0; i < river.Count - 1; i++) {
        Vector2 firstRiverPoint = getHexRiverPoint(river[i], hexSize, map);
        Vector2 secondRiverPoint = getHexRiverPoint(river[i + 1], hexSize, map);
        Vector2 direction = secondRiverPoint - firstRiverPoint;
        Vector2 normal = new Vector2(-direction.y, direction.x).normalized;
        int pointsNumber = Mathf.CeilToInt(direction.magnitude / riverDensity);

        for (int pointIndex = 0; pointIndex < pointsNumber; pointIndex++) {
          Vector3 derivativeDirection;
          float t = pointIndex * 1.0f / pointsNumber;
          Vector2 newPoint = Vector2.Lerp(firstRiverPoint, secondRiverPoint, t);
          float scaleShiftFactor = pointIndex <= pointsNumber / 2 ? pointIndex * 2.0f / pointsNumber : (pointsNumber - pointIndex) * 2.0f / pointsNumber;
          newPoint = Vector2.Lerp(firstRiverPoint, secondRiverPoint, t);
          float shift = Mathf.PerlinNoise(perlinNoiseX, riverPoints.Count * perlinNoiseStep) * 0.4f - 0.2f;
          newPoint += normal * shift * scaleShiftFactor;

          if (i != 0 || pointIndex != 0) {
            if (pointIndex == 0) {
              float nextPointT = (pointIndex + 1) * 1.0f / pointsNumber;
              Vector2 nextPoint = Vector2.Lerp(firstRiverPoint, secondRiverPoint, nextPointT);
              float nextScaleShiftFactor = pointIndex + 1 <= pointsNumber / 2 ? (pointIndex + 1) * 2.0f / pointsNumber : (pointsNumber - pointIndex - 1) * 2.0f / pointsNumber;
              float nextPointShift = Mathf.PerlinNoise(perlinNoiseX, (riverPoints.Count + 1) * perlinNoiseStep) * 0.4f - 0.2f;
              nextPoint += normal * nextPointShift * nextScaleShiftFactor;

              Vector2 previousPoint = riverPoints[riverPoints.Count - 1];

              float derivativeT = 0.5f;
              derivativeDirection = 2f * (1f - derivativeT) * (newPoint - previousPoint) + 2f * derivativeT * (nextPoint - newPoint);

              newPoint = Vector2.Lerp(Vector2.Lerp(previousPoint, newPoint, 0.5f), Vector2.Lerp(newPoint, nextPoint, 0.5f), 0.5f);
            } else {
              Vector2 previousPoint = riverPoints[riverPoints.Count - 1];
              derivativeDirection = newPoint - previousPoint;
            }

            Vector3 perpendicular = new Vector3(-derivativeDirection.y, derivativeDirection.x).normalized;
            Vector3 actualNewPoint = new Vector3(newPoint.x, newPoint.y, 0);
            Vector3 firstShorePoint = actualNewPoint + perpendicular * riverThickness;
            Hex firstPointHex = map.getHex(HexMathHelper.worldToHexCoords(firstShorePoint, hexSize));
            float firstShorePointHeight = TerrainHelper.getHeight(heightSeed, firstShorePoint, firstPointHex, hexSize, elevatedHeightmap, mountainHeightmap);
            firstShorePoint += new Vector3(0, 0, firstShorePointHeight);

            Vector3 secondShorePoint = actualNewPoint - perpendicular * riverThickness;
            Hex secondPointHex = map.getHex(HexMathHelper.worldToHexCoords(secondShorePoint, hexSize));
            float secondShorePointHeight = TerrainHelper.getHeight(heightSeed, secondShorePoint, secondPointHex, hexSize, elevatedHeightmap, mountainHeightmap);
            secondShorePoint += new Vector3(0, 0, secondShorePointHeight);

            riverShorePoints.Add(firstShorePoint);
            riverShorePoints.Add(secondShorePoint);
          }

          Hex newPointHex = map.getHex(HexMathHelper.worldToHexCoords(newPoint, hexSize));
          float newPointHeight = TerrainHelper.getHeight(heightSeed, newPoint, newPointHex, hexSize, elevatedHeightmap, mountainHeightmap);
          riverPoints.Add(new Vector3(newPoint.x, newPoint.y, newPointHeight));
        }
      }
      Vector3 veryFirstShorePoint = riverShorePoints[0];
      Vector3 verySecondShorePoint = riverShorePoints[1];
      Vector3 firstPointShift = new Vector2(riverPoints[0].x, riverPoints[0].y) - new Vector2(riverPoints[1].x, riverPoints[1].y);
      Vector3 missingFirstPoint = verySecondShorePoint + firstPointShift;
      float missingFirstPointHeight = TerrainHelper.getHeight(heightSeed, missingFirstPoint,
        map.getHex(HexMathHelper.worldToHexCoords(missingFirstPoint, hexSize)), hexSize, elevatedHeightmap, mountainHeightmap);
      missingFirstPoint += new Vector3(0, 0, missingFirstPointHeight);
      riverShorePoints.Insert(0, missingFirstPoint);

      Vector3 missingSecondPoint = veryFirstShorePoint + firstPointShift;
      float missingSecondPointHeight = TerrainHelper.getHeight(heightSeed, missingSecondPoint,
        map.getHex(HexMathHelper.worldToHexCoords(missingSecondPoint, hexSize)), hexSize, elevatedHeightmap, mountainHeightmap);
      missingSecondPoint += new Vector3(0, 0, missingSecondPointHeight);
      riverShorePoints.Insert(0, missingSecondPoint);

      riverPoints.AddRange(riverShorePoints);
      //allRriversPoints.AddRange(riverPoints);
      allRriversPoints.AddRange(riverShorePoints);
      for (int i = 0, vertextIndex = 0; i <= riverShorePoints.Count - 4; i += 2, vertextIndex += 6) {
        vertices.Add(riverShorePoints[i + 2]);
        vertices.Add(riverShorePoints[i + 1]);
        vertices.Add(riverShorePoints[i]);

        vertices.Add(riverShorePoints[i + 3]);
        vertices.Add(riverShorePoints[i + 1]);
        vertices.Add(riverShorePoints[i + 2]);

        for (int triangleIndex = vertextIndex; triangleIndex < vertextIndex + 6; triangleIndex++) {
          triangles.Add(triangleIndex);
          colours.Add(Color.blue);
        }
      }
    }

    Mesh mesh = new Mesh();
    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    mesh.name = "Rivers";
    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangles.ToArray();
    mesh.colors32 = colours.ToArray();
    mesh.RecalculateNormals();
    mesh.RecalculateBounds();

    return allRriversPoints;
  }

  private static Vector2 getHexRiverPoint(Vector2 hexPosition, float hexSize, Map map) {

    Vector2 hexCenter = HexMathHelper.hexToWorldCoords(hexPosition, hexSize);
    Hex riverHex = map.hexes.Find(hex => hex.getPosition() == hexPosition);
    if (riverHex == null) {
      return hexCenter;
    }
    return new Vector2(hexCenter.x + riverHex.riverPoint.x * hexSize, hexCenter.y + riverHex.riverPoint.y * hexSize);
  }

  public static void shuffleList(List<Vector2> list) {
    for (int i = list.Count; i > 1; i--) {
      int position = Random.RandomRange(0, list.Count - 1);
      Vector2 temporary = list[i - 1];
      list[i - 1] = list[position];
      list[position] = temporary;
    }
  }

  public static void shuffleHexList(List<Hex> list) {
    for (int i = list.Count; i > 1; i--) {
      int position = Random.RandomRange(0, list.Count - 1);
      Hex temporary = list[i - 1];
      list[i - 1] = list[position];
      list[position] = temporary;
    }
  }
}