using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColourHelper {
  public static Color32 getHexColor(Hex hex) {
    if (hex == null || hex.biome == null) {
      return Color.blue;
    }

    switch (hex.biome.name) {
      case "Alpine barren": return Color.grey;
      case "Cold desert": return Color.cyan;
      case "Moss bog": return new Color32(127, 51, 0, 255);
      case "Fresh lake": return Color.blue;
      case "Grassland": return Color.green;
      case "Hot desert": return new Color32(182, 255, 0, 255);
      case "Salt lake": return new Color32(0, 255, 255, 255);
      case "Savanna grassland": return new Color32(173, 255, 43, 255);
      case "Marsh": return new Color32(127, 151, 0, 255);
      case "Scrubland": return new Color32(255, 250, 117, 255);
      case "Tropical rainforest": return new Color32(38, 127, 0, 255);
      case "Tundra": return new Color32(175, 183, 165, 255);
      case "Volcanic desert": return new Color32(78, 81, 74, 255);
    }

    switch (hex.biome.parent) {
      case "Alpine barren": return Color.grey;
      case "Cold desert": return Color.cyan;
      case "Moss bog": return new Color32(127, 51, 0, 255);
      case "Fresh lake": return Color.blue;
      case "Grassland": return Color.green;
      case "Hot desert": return new Color32(182, 255, 0, 255);
      case "Salt lake": return new Color32(0, 255, 255, 255);
      case "Savanna grassland": return new Color32(173, 255, 43, 255);
      case "Marsh": return new Color32(127, 151, 0, 255);
      case "Scrubland": return new Color32(255, 250, 117, 255);
      case "Tropical rainforest": return new Color32(38, 127, 0, 255);
      case "Tundra": return new Color32(175, 183, 165, 255);
      case "Volcanic desert": return new Color32(78, 81, 74, 255);
    }

    return Color.blue;
  }

  public static int biomeRarity (Hex hex) {
    if (hex == null || hex.biome == null) {
      return 0;
    }

    switch (hex.biome.name) {
      case "Fresh lake": return 1;
      case "Grassland": return 2;
      case "Marsh": return 3;
      case "Tropical rainforest": return 4;
      case "Savanna grassland": return 5;
      case "Tundra": return 6;
      case "Alpine barren": return 7;
      case "Cold desert": return 8;
      case "Moss bog": return 9;
      case "Hot desert": return 10;
      case "Salt lake": return 11;
      case "Scrubland": return 12; 
      case "Volcanic desert": return 13;
    }

    switch (hex.biome.parent) {
      case "Fresh lake": return 1;
      case "Grassland": return 2;
      case "Marsh": return 3;
      case "Tropical rainforest": return 4;
      case "Savanna grassland": return 5;
      case "Tundra": return 6;
      case "Alpine barren": return 7;
      case "Cold desert": return 8;
      case "Moss bog": return 9;
      case "Hot desert": return 10;
      case "Salt lake": return 11;
      case "Scrubland": return 12;
      case "Volcanic desert": return 13;
    }

    return 0;
  }

  public static Color combineColors(List<Color> colors) {
    Color result = new Color(0, 0, 0, 0);
    foreach (Color color in colors) {
      result += color;
    }
    result = result / colors.Count;

    return result;
  }

  public static bool isWaterColor(Color32 color) {
    return color.r == 0 && color.g == 0 && color.b == 255;
  }
}