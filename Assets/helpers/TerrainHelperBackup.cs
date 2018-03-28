using UnityEngine;

public class TerrainHelperBackup {

  public static float getHeight(Vector3 vertex, Hex hex, float[,] surfaceCurving,
    float mapLeft, float mapBottom, float hexSize, Texture2D elevatedHeightmap, Texture2D mountainHeightmap) {
    mapLeft = Mathf.Floor(mapLeft);
    mapBottom = Mathf.Floor(mapBottom);
    if (hex == null) {
      //water
      return 0.2f + getTerrainCurvingHeight(vertex, surfaceCurving, mapLeft, mapBottom);
    }
    Vector2 hexCenter = HexMathHelper.hexToWorldCoords(new Vector2(hex.x, hex.y), hexSize);
    Vector2 hexShift = hexCenter - new Vector2(vertex.x, vertex.y);
    Vector2 heightMapCoordinate = new Vector2(0.5f + hexShift.x / hexSize, 0.5f + hexShift.y / hexSize);

    float terrainCurvingHeight = getTerrainCurvingHeight(vertex, surfaceCurving, mapLeft, mapBottom);
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

  public static float[,] generateSurfaceCurving(float top, float left, float bottom, float right) {
    top = Mathf.Ceil(top);
    left = Mathf.Floor(left);
    bottom = Mathf.Floor(bottom);
    right = Mathf.Ceil(right);

    float[,] surfaceCurving = new float[(int)((right - left) * 2 + 1), (int)((top - bottom) * 2 + 1)];
    for (int x = 0; x <= (int)(right - left) * 2; x++) {
      for (int y = 0; y <= (int)(top - bottom) * 2; y++) {
        surfaceCurving[x, y] = Random.Range(-0.2f, 0.2f);
      }
    }

    return surfaceCurving;
  }

  public static float getTerrainCurvingHeight(Vector2 vertex, float[,] surfaceCurving, float mapLeft, float mapBottom) {
    int left = Mathf.FloorToInt(vertex.x * 2);
    int right = Mathf.CeilToInt(vertex.x * 2);
    int bottom = Mathf.FloorToInt(vertex.y * 2);
    int top = Mathf.CeilToInt(vertex.y * 2);

    float f00 = getSurfaceCurving(left, bottom, surfaceCurving, mapLeft, mapBottom);
    float f10 = getSurfaceCurving(right, bottom, surfaceCurving, mapLeft, mapBottom);
    float f01 = getSurfaceCurving(left, top, surfaceCurving, mapLeft, mapBottom);
    float f11 = getSurfaceCurving(right, top, surfaceCurving, mapLeft, mapBottom);

    float x = vertex.x * 2 - left;
    float y = vertex.y * 2 - bottom;

    return f00 * (1 - x) * (1 - y) + f10 * x * (1 - y) + f01 * (1 - x) * y + f11 * x * y;
  }

  public static float getSurfaceCurving(int x, int y, float[,] surfaceCurving, float mapLeft, float mapBottom) {
    try {
      float vertexSurfaceCurving = surfaceCurving[x - Mathf.FloorToInt(mapLeft), y - Mathf.FloorToInt(mapBottom)];
    } catch (System.Exception e) {
      return 0f;
    }
    return surfaceCurving[x - Mathf.FloorToInt(mapLeft), y - Mathf.FloorToInt(mapBottom)];
  }
}