using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Map
{
  public List<Hex> hexes = new List<Hex>();
  public List<Region> regions = new List<Region>();

  public Hex getHex(Vector2 hexCoordinates)
  {
    return this.hexes.Find(hex => hex.x == hexCoordinates.x && hex.y == hexCoordinates.y);
  }
}