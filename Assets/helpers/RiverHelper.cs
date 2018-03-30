using UnityEngine;
using System.Collections.Generic;

public class RiverHelper {
  public static List<List<Hex>> generateRivers(Map map) {
    List<List<Hex>> rivers = new List<List<Hex>>();
    List<Hex> firstRiver = new List<Hex>();
    firstRiver.Add(map.hexes[0]);
    firstRiver.Add(map.hexes[1]);
    firstRiver.Add(map.hexes[2]);
    firstRiver.Add(map.hexes[3]);
    rivers.Add(firstRiver);

    List<Hex> secondRiver = new List<Hex>();
    secondRiver.Add(map.hexes[5]);
    secondRiver.Add(map.hexes[6]);
    secondRiver.Add(map.hexes[7]);
    secondRiver.Add(map.hexes[8]);
    rivers.Add(secondRiver);


    List<Hex> thirdRiver = new List<Hex>();
    thirdRiver.Add(map.hexes[10]);
    thirdRiver.Add(map.hexes[11]);
    thirdRiver.Add(map.hexes[12]);
    rivers.Add(thirdRiver);

    return rivers;
  }

  public static List<Vector3> getRiversMesh(List<List<Hex>> rivers, float riverDensity, float hexSize) {
    float riverZ = -1;
    float perlinNoiseX = 15.004f;
    float perlinNoiseStep = 0.1f;
    List<Vector3> allRriversPoints = new List<Vector3>();
    float riverThickness = riverDensity / 4;

    foreach (List<Hex> river in rivers) {
      List<Vector3> riverShorePoints = new List<Vector3>();
      List<Vector3> riverPoints = new List<Vector3>();
      for (int i = 0; i < river.Count - 1; i++) {
        Vector2 firstRiverPoint = getHexRiverPoint(river[i], hexSize);
        Vector2 secondRiverPoint = getHexRiverPoint(river[i + 1], hexSize);
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
            Vector3 actualNewPoint = new Vector3(newPoint.x, newPoint.y, riverZ);
            Vector3 firstShorePoint = actualNewPoint + perpendicular * riverThickness;
            Vector3 secondShorePoint = actualNewPoint - perpendicular * riverThickness;
            riverShorePoints.Add(firstShorePoint);
            riverShorePoints.Add(secondShorePoint);
          }

          riverPoints.Add(new Vector3(newPoint.x, newPoint.y, riverZ));
        }
      }
      Vector3 veryFirstShorePoint = riverShorePoints[0];
      Vector3 verySecondShorePoint = riverShorePoints[1];
      Vector3 firstPointShift = riverPoints[0] - riverPoints[1];
      riverShorePoints.Insert(0, verySecondShorePoint + firstPointShift);
      riverShorePoints.Insert(0, veryFirstShorePoint + firstPointShift);

      riverPoints.AddRange(riverShorePoints);
      allRriversPoints.AddRange(riverPoints);
    }

    return allRriversPoints;
  }

  private static Vector2 getHexRiverPoint(Hex hex, float hexSize) {
    Vector2 hexCenter = HexMathHelper.hexToWorldCoords(new Vector2(hex.x, hex.y), hexSize);

    return new Vector2(hexCenter.x + hex.riverPoint.x * hexSize, hexCenter.y + hex.riverPoint.y * hexSize);
  }
}