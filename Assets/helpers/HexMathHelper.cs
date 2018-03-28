using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class HexMathHelper {

  public static Vector2 LEFT = new Vector2(-1, 0);
  public static Vector2 RIGHT = new Vector2(1, 0);
  public static Vector2 TOP_LEFT = new Vector2(0, 1);
  public static Vector2 TOP_RIGHT = new Vector2(1, 1);
  public static Vector2 BOTTOM_LEFT = new Vector2(-1, -1);
  public static Vector2 BOTTOM_RIGHT = new Vector2(0, -1);

  public static Vector2[] DIRECTION_ARRAY = { RIGHT, TOP_RIGHT, TOP_LEFT, LEFT, BOTTOM_LEFT, BOTTOM_RIGHT };

  public static float getOuterRadius(float hexSize) {
    return hexSize / 2 / Mathf.Cos(Mathf.PI / 6);
  }

  public static List<Vector2> getHexCorners(float hexSize) {
    List<Vector2> corners = new List<Vector2>();

    float outerRadius = getOuterRadius(hexSize);
    corners.Add(new Vector2(0, outerRadius));
    corners.Add(new Vector2(hexSize / 2, outerRadius / 2));
    corners.Add(new Vector2(hexSize / 2, - outerRadius / 2));
    corners.Add(new Vector2(0, - outerRadius));
    corners.Add(new Vector2(-hexSize / 2, -outerRadius / 2));
    corners.Add(new Vector2(-hexSize / 2, outerRadius / 2));

    return corners;
  }

  public static Vector2 getHexXStepVector(float hexSize) {
		return hexSize * getHexGridXUnitVector ();
	}

	public static Vector2 getHexYStepVector(float hexSize) {
		return hexSize * getHexGridYUnitVector();
	}

	public static Vector2 getHexGridXUnitVector() {
		return new Vector2 (1, 0);
	}

	public static Vector2 getHexGridYUnitVector() {
		return new Vector2 (Mathf.Cos(2 * Mathf.PI / 3), Mathf.Sin(2 * Mathf.PI / 3));
	}

	public static int hexDistance (Vector2 hexOne, Vector2 hexTwo) {
		return 
			(int)(Mathf.Abs (hexOne.x - hexTwo.x) +
				Mathf.Abs (hexOne.x - hexTwo.x - (hexOne.y - hexTwo.y)) +
				Mathf.Abs (hexOne.y - hexTwo.y)) / 2;
	}

	public static Vector2 worldToHexCoords(Vector2 worldCoords, float hexSize) {
		worldCoords = worldCoords / hexSize;
		float hexY = (worldCoords.y * getHexGridXUnitVector ().x - worldCoords.x * getHexGridXUnitVector ().y) / (getHexGridXUnitVector ().x * getHexGridYUnitVector ().y - getHexGridYUnitVector ().x * getHexGridXUnitVector ().y);
		float hexX = (worldCoords.x - hexY * getHexGridYUnitVector().x) / getHexGridXUnitVector().x;
		//float hexX = worldCoords.x / (hexSize * (HexMathHelper.getHexGridXUnitVector ().x + HexMathHelper.getHexGridYUnitVector ().x));
		//float hexY = worldCoords.x / (hexSize * (HexMathHelper.getHexGridXUnitVector ().x + HexMathHelper.getHexGridYUnitVector ().x));
		Vector2 hexCoords = cubeToHex (roundToHex (hexToCube (new Vector2 (hexX, hexY))));

		return new Vector2 (Mathf.RoundToInt (hexCoords.x), Mathf.RoundToInt (hexCoords.y));
	}

	public static Vector2 hexToWorldCoords(Vector2 hexCoords, float hexSize) {
		float worldX = hexCoords.x * HexMathHelper.getHexGridXUnitVector ().x + hexCoords.y * HexMathHelper.getHexGridYUnitVector ().x;
		float worldY = hexCoords.x * HexMathHelper.getHexGridXUnitVector ().y + hexCoords.y * HexMathHelper.getHexGridYUnitVector ().y;

		return new Vector2 (worldX * hexSize, worldY * hexSize);
	}

	public static Vector3 hexToCube (Vector2 hexCoords) {
		return new Vector3 (hexCoords.x, hexCoords.y, hexCoords.x - hexCoords.y);
	}

	public static Vector2 cubeToHex (Vector3 cubeCoords) {
		return cubeCoords;
	}

	public static Vector3 roundToHex(Vector3 cubeFractionalCoords) {
		float roundedX = Mathf.Round (cubeFractionalCoords.x);
		float roundedY = Mathf.Round (cubeFractionalCoords.y);
		float roundedZ = Mathf.Round (cubeFractionalCoords.z);

		if (-roundedX + roundedY + roundedZ == 0) {
			return new Vector3 (roundedX, roundedY, roundedZ);
		}

		float xDifference = Mathf.Abs (roundedX - cubeFractionalCoords.x);
		float yDifference = Mathf.Abs (roundedY - cubeFractionalCoords.y);
		float zDifference = Mathf.Abs (roundedZ - cubeFractionalCoords.z);

		if (xDifference > yDifference && xDifference > zDifference) {
			roundedX = roundedY + roundedZ;
		} else if (yDifference > zDifference) {
			roundedY = roundedX - roundedZ;
		} else {
			roundedZ = roundedX - roundedY;
		}

		return new Vector3 (roundedX, roundedY, roundedZ);
	}

	public static List<Vector2> getHexNeighbours(Vector2 centerHex, int distanceFromCenter) {
		List<Vector2> neighbours = new List<Vector2> ();

		for (int x = -distanceFromCenter; x <= distanceFromCenter; x++) {
			for (int y = -distanceFromCenter; y <= distanceFromCenter; y++) {
				if(Mathf.Abs(-x + y) <= distanceFromCenter && (x != centerHex.x || y!= centerHex.y)){
					neighbours.Add (new Vector2 (centerHex.x + x, centerHex.y + y));
				}
			}
		}

		return neighbours;
	}

  public static List<Vector2> getCircleAround(Vector2 centerHex, int radius)
  {
    List<Vector2> hexCircle = new List<Vector2>();

    Vector2 currentHex = centerHex + radius * BOTTOM_LEFT;
    for(int i = 0; i < 6; i++)
    {
      for(int j = 0; j < radius; j++)
      {
        currentHex += DIRECTION_ARRAY[i];
        hexCircle.Add(new Vector2(currentHex.x, currentHex.y));
      }
    }

    return hexCircle;
  }



}
