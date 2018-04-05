using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Topology;

public class TerrainHelper {

  public static float getHeight(float heightSeed, MapVertex mapVertex, float hexSize, Texture2D elevatedHeightmap, Texture2D mountainHeightmap) {
    Vector3 vertex = mapVertex.toVector3();
    Hex hex = mapVertex.hex;

    return getHeight(heightSeed, vertex, hex, hexSize, elevatedHeightmap, mountainHeightmap);
  }

  public static float getHeight(float heightSeed, Vector3 vertex, Hex hex, float hexSize, Texture2D elevatedHeightmap, Texture2D mountainHeightmap) {
    if (hex == null) {
      //water
      return 0.2f + getTerrainCurvingHeight(vertex, heightSeed);
    }
    Vector2 hexCenter = HexMathHelper.hexToWorldCoords(new Vector2(hex.x, hex.y), hexSize);
    Vector2 hexShift = hexCenter - new Vector2(vertex.x, vertex.y);
    Vector2 heightMapCoordinate = new Vector2(0.5f + hexShift.x / hexSize, 0.5f + hexShift.y / hexSize);

    float terrainCurvingHeight = getTerrainCurvingHeight(vertex, heightSeed);
    if (hex.hasTag(RiverHelper.FRESH_LAKE_TAG)) {
      if (hex.hasTag("normal")) {
        return 0.1f;
      }
      if (hex.hasTag("low")) {
        return 0.15f;
      }
    }

    if (hex.hasTag("high")) {
      return terrainCurvingHeight - mountainHeightmap.GetPixelBilinear(heightMapCoordinate.x, heightMapCoordinate.y).grayscale * 1;
    }

    if (hex.hasTag("elevated")) {
      return terrainCurvingHeight - elevatedHeightmap.GetPixelBilinear(heightMapCoordinate.x, heightMapCoordinate.y).grayscale * 1;
      //return  - Mathf.Abs(terrainCurvingHeight) * 2f;
    }

    if (hex.hasTag("normal")) {
      return terrainCurvingHeight;
    }
    if (hex.hasTag("low")) {
      return 0.05f + Mathf.Abs(terrainCurvingHeight);
    }

    return terrainCurvingHeight;
  }

  public static Vector3 getRiverHeightAdjustment(Vector3 vertex) {
    return new Vector3(0, 0, 0.1f);
  }

  public static float getTerrainCurvingHeight(Vector2 vertex, float heightSeed) {
    return (Mathf.PerlinNoise((heightSeed + vertex.x), (heightSeed + vertex.y)) * 0.4f) - 0.2f;
  }

}