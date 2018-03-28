using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AtlasTextureHelper {
  private static int atlasSize = 4096;
  private static int textureSize = 512;
  public static List<Vector2> getUVForTexture(int xInAtlas, int yInAtlas) {
    List<Vector2> uv = new List<Vector2>();
    Vector2 textureCenter = new Vector2(xInAtlas * textureSize - textureSize / 2, yInAtlas * textureSize - textureSize / 2);
    Vector2 topCorner = textureCenter + new Vector2(0, textureSize / 2);
    Vector2 bottomCorner = textureCenter + new Vector2(0, -textureSize / 2);
    Vector2 rightTopCorner = textureCenter + new Vector2((textureSize / 2) * Mathf.Cos(Mathf.PI / 6), textureSize / 4);
    Vector2 rightBottomCorner = textureCenter + new Vector2((textureSize / 2) * Mathf.Cos(Mathf.PI / 6), -textureSize / 4);
    Vector2 leftTopCorner = textureCenter + new Vector2(-(textureSize / 2) * Mathf.Cos(Mathf.PI / 6), textureSize / 4);
    Vector2 leftBottomCorner = textureCenter + new Vector2(-(textureSize / 2) * Mathf.Cos(Mathf.PI / 6), -textureSize / 4);

    uv.Add(textureCenter);
    uv.Add(topCorner);
    uv.Add(rightTopCorner);

    uv.Add(textureCenter);
    uv.Add(rightTopCorner);
    uv.Add(rightBottomCorner);

    uv.Add(textureCenter);
    uv.Add(rightBottomCorner);
    uv.Add(bottomCorner);

    uv.Add(textureCenter);
    uv.Add(bottomCorner);
    uv.Add(leftBottomCorner);

    uv.Add(textureCenter);
    uv.Add(leftBottomCorner);
    uv.Add(leftTopCorner);

    uv.Add(textureCenter);
    uv.Add(leftTopCorner);
    uv.Add(topCorner);

    return convertPixelsListToUV(uv);
  }

  private static List<Vector2> convertPixelsListToUV(List<Vector2> pixelsList) {
    List<Vector2> uv = new List<Vector2>();
    foreach(Vector2 pixels in pixelsList) {
      uv.Add(convertPixelsToUV(pixels));
    }

    return uv;
  }

  private static Vector2 convertPixelsToUV(Vector2 pixels) {
    return new Vector2(Mathf.InverseLerp(0f, (float)atlasSize, pixels.x), Mathf.InverseLerp(0f, (float)atlasSize, pixels.y));
  }
}