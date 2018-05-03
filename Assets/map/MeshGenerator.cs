using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Topology;

public class MeshGenerator {

  public float hexSize;
  public Map map;
  public float polygonDensity;
  public float riverDensity;
  public float heightSeed;
  public Texture2D elevatedHeightmap;
  public Texture2D mountainHeightmap;
  public GameObject rivers;

  public MeshGenerator(float hexSize, Map map, float polygonDensity, float riverDensity,
    float heightSeed, Texture2D elevatedHeightmap, Texture2D mountainHeightmap, GameObject rivers) {
    this.hexSize = hexSize;
    this.map = map;
    this.polygonDensity = polygonDensity;
    this.heightSeed = heightSeed;
    this.riverDensity = riverDensity;
    this.elevatedHeightmap = elevatedHeightmap;
    this.mountainHeightmap = mountainHeightmap;
    this.rivers = rivers;
  }

  public MapMesh generateMapMesh() {
    List<List<Vector2>> riverPoints = this.generateRiverPoints();
    TriangleNet.Mesh mapTriangulation = this.triangulateMap(map, riverPoints);
    MapMesh terrain = this.generateTerrain(map, riverPoints, mapTriangulation);
    List<Color32> terrainColors = getTerrainColours(terrain.landMesh.vertices);
    Debug.Log(terrainColors.Count);
    Debug.Log(terrain.landMesh.vertices.Length);
    Debug.Log(terrain.innerWaterMesh.vertices.Length);
    Debug.Log(terrain.oceanMesh.vertices.Length);
    terrain.landMesh.colors32 = terrainColors.ToArray();

    return terrain;
  }

  public MapMesh generateTerrain(Map map, List<List<Vector2>> riverPoints, TriangleNet.Mesh mapTriangulation) {
    MapMesh terrain = new MapMesh();

    List<Vector3> landVertices = new List<Vector3>();
    List<Vector3> oceanVertices = new List<Vector3>();
    List<Vector3> innerWaterVertices = new List<Vector3>();
    Dictionary<Vector2, float> mapVerticesHeights = new Dictionary<Vector2, float>();

    foreach (Triangle triangle in mapTriangulation.Triangles) {
      List<MapVertex> mapVertices = triangle.vertices
        .Select(vertex => vertex is MapVertex ? (MapVertex)vertex : convertToMapVertex(vertex))
        .Reverse()
        .ToList();

      List<Vector3> underWaterShifts = new List<Vector3>(new Vector3[3]);

      List<bool> needsShoreAdjustments = new List<bool>(new bool[3]);

      if (mapVertices.Any(mapVertice => mapVertice.hex == null)) {
        for (int i = 0; i < mapVertices.Count;i++) {
          if(mapVertices[i].hex != null) {
            needsShoreAdjustments[i] = true;
          }
        }
      }

      MapVertex shoreMapVertex = mapVertices.FirstOrDefault(mapVertex => mapVertex.hex != null && (mapVertex.hex.biome == null || mapVertex.hex.biome.name != "Fresh lake"));
      if (shoreMapVertex != null) {
        Vector2 shoreCenter = HexMathHelper.hexToWorldCoords(new Vector2(shoreMapVertex.hex.x, shoreMapVertex.hex.y), this.hexSize);
        //if shore triangle push it underwater to create shore slope
        for (int i = 0; i < mapVertices.Count; i++) {
          if (mapVertices[i].hex == null) {
            underWaterShifts[i] = (new Vector2((float)mapVertices[i].x, (float)mapVertices[i].y) - shoreCenter).normalized * 0.3f;
            underWaterShifts[i] += new Vector3(0, 0, 0.03f);
          }
        }
      }

      List<Vector3> triangleVertices = new List<Vector3>();
      for (int i = 0; i < mapVertices.Count; i++) {
        float height = TerrainHelper.getHeight(heightSeed, mapVertices[i], this.hexSize, elevatedHeightmap, mountainHeightmap);
        Vector3 newVertex = mapVertices[i].toVector3() + underWaterShifts[i] + new Vector3(0, 0, height);

        if (!mapVerticesHeights.ContainsKey(newVertex) && needsShoreAdjustments[i]) {
          newVertex.z = Random.Range(0.08f, 0.13f);
          mapVerticesHeights[newVertex] = newVertex.z;
        }

        if (mapVertices[i].isRiver && (mapVertices[i].hex == null || !mapVertices[i].hex.hasTag(RiverHelper.FRESH_LAKE_TAG))) {
          newVertex += TerrainHelper.getRiverHeightAdjustment(newVertex);
        }

        triangleVertices.Add(newVertex);
      }

      if (mapVertices.All(mapVertex => mapVertex.hex == null)) {
        oceanVertices.AddRange(triangleVertices);
      } else if (mapVertices.All(mapVertex => mapVertex.isRiver)) {
        innerWaterVertices.AddRange(triangleVertices);
      } else if (mapVertices.All(mapVertex => mapVertex.hex != null && mapVertex.hex.hasTag(RiverHelper.FRESH_LAKE_TAG))) { 
        innerWaterVertices.AddRange(triangleVertices);
      } else {
        landVertices.AddRange(triangleVertices);
      }
    }

    //smooth shore
    for (int i = 0; i < landVertices.Count; i++) {
      Vector3 vertex = landVertices[i];
      if (mapVerticesHeights.ContainsKey(vertex)) {
        vertex.z = mapVerticesHeights[landVertices[i]];
        landVertices[i] = vertex;
      }
    }

    terrain.landMesh = this.createMesh("Land", landVertices);
    terrain.oceanMesh = this.createMesh("Ocean", oceanVertices);
    terrain.innerWaterMesh = this.createMesh("Lakes and Rivers", innerWaterVertices);

    return terrain;
  }

  public List<Color32> getTerrainColours(Vector3[] vertices) {
    List<Color32> terrainColors = new List<Color32>();
    for (int i = 0; i <= vertices.Length - 3; i+=3) {
      List<Vector3> triangleVertices = new List<Vector3>() { vertices[i], vertices[i + 1], vertices[i + 2] };
      List<Hex> triangleHexes = triangleVertices
        .Select(vertex => this.map.getHex(HexMathHelper.worldToHexCoords(vertex, this.hexSize)))
        .ToList();
      Color resultTriangleColor = ColourHelper.WATER_COLOUR;
      List<Color> triangleColors = triangleHexes.Select(hex => ColourHelper.getHexColor(hex)).ToList();
      if (triangleColors.Count(color => color == triangleColors[0]) == 3) {
        resultTriangleColor = triangleColors[0];
      } else if (triangleColors.Any(color => ColourHelper.isWaterColor(color))) {
        if (triangleHexes.Any(hex => hex == null)) {
          resultTriangleColor = Color.yellow;
        } else {
          resultTriangleColor = new Color32(70, 100, 0, 255);
        }
      } else {
        Hex rarestHex = triangleHexes.Aggregate((currMin, hex) =>
        ColourHelper.biomeRarity(hex) < ColourHelper.biomeRarity(hex) ?
        hex : currMin);

        resultTriangleColor = ColourHelper.getHexColor(rarestHex);
      }

      float averageHeight = triangleVertices.Sum(vertex => vertex.z) / 3;
      if (averageHeight <= -0.4) {
        resultTriangleColor = Color.white;
      }

      for (int j = 0; j < 3; j++) {
        terrainColors.Add(resultTriangleColor);
      }
    }

    return terrainColors;
  }

  public MapVertex convertToMapVertex(Vertex vertex) {
    Hex vertexHex = map.getHex(HexMathHelper.worldToHexCoords(new Vector2((float)vertex.x, (float)vertex.y), this.hexSize));

    return new MapVertex(vertex.x, vertex.y, vertexHex);
  }

  public Mesh createMesh(string meshName, List<Vector3> vertices) {
    return this.createMesh(meshName, vertices, null);
  }

  public Mesh createMesh(string meshName, List<Vector3> vertices, List<Color32> colours) {
    Mesh mesh = new Mesh();
    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    mesh.name = meshName;
    mesh.vertices = vertices.ToArray();
    mesh.triangles = simplestTriangles(vertices.Count);
    mesh.colors32 = colours != null ? colours.ToArray() : simplestColours(ColourHelper.WATER_COLOUR, vertices.Count);
    mesh.RecalculateNormals();
    mesh.RecalculateBounds();

    return mesh;
  }

  public TriangleNet.Mesh triangulateMap(Map map, List<List<Vector2>> riverPoints) {
    MapBounds mapBounds = this.calculateMapBounds(map, this.hexSize);
    PolygonCollider2D riverCollider = generateRiversCollider(riverPoints);

    Polygon polygon = new Polygon();

    polygon.Add(new Vertex(mapBounds.left, mapBounds.bottom));
    for (float y = mapBounds.bottom; y <= mapBounds.top;) {
      Hex rowLeftHex = map.getHex(HexMathHelper.worldToHexCoords(new Vector2(mapBounds.left, y), this.hexSize));
      polygon.Add(new MapVertex(mapBounds.left, y, rowLeftHex));
      for (float x = mapBounds.left; x < mapBounds.right;) {
        x += polygonDensity * (1 + Random.Range(-0.4f, 0.4f));
        if (x > mapBounds.right) {
          x = mapBounds.right;
        }
        float yDispersion = y == mapBounds.bottom || y == mapBounds.top ? 0 : polygonDensity * Random.Range(-0.3f, 0.3f);
        Hex vertexHex = map.getHex(HexMathHelper.worldToHexCoords(new Vector2(x, y + yDispersion), this.hexSize));
        if (!riverCollider.OverlapPoint(new Vector2(x, y + yDispersion))) {
          polygon.Add(new MapVertex(x, y + yDispersion, vertexHex));
        }
      }
      if (y == mapBounds.top) {
        break;
      }
      y += polygonDensity * (1 + Random.Range(-0.3f, 0.3f));
      if (y > mapBounds.top) {
        y = mapBounds.top;
      }
    }

    foreach (List<Vector2> river in riverPoints) {
      foreach (Vector2 riverPoint in river) {
        Hex vertexHex = map.getHex(HexMathHelper.worldToHexCoords(riverPoint, this.hexSize));
        polygon.Add(new MapVertex(riverPoint.x, riverPoint.y, vertexHex, true));
      }
    }

    TriangleNet.Meshing.ConstraintOptions options =
    new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };
    return (TriangleNet.Mesh)polygon.Triangulate(options);
  }

  private MapBounds calculateMapBounds(Map map, float hexSize) {
    float minY = map.hexes[0].y;
    float maxY = map.hexes[0].y;
    Vector2 leftMostHexCoordinates = new Vector2(map.hexes[0].x, map.hexes[0].y);
    Vector2 rightMostHexCoordinates = new Vector2(map.hexes[0].x, map.hexes[0].y);
    foreach (Hex hex in map.hexes) {
      if (hex.y > maxY) {
        maxY = hex.y;
      }
      if (hex.y < minY) {
        minY = hex.y;
      }

      float xCoordinate = hex.x - 0.5f * hex.y;
      if (xCoordinate < leftMostHexCoordinates.x - 0.5f * leftMostHexCoordinates.y) {
        leftMostHexCoordinates = new Vector2(hex.x, hex.y);
      }
      if (xCoordinate > rightMostHexCoordinates.x - 0.5f * rightMostHexCoordinates.y) {
        rightMostHexCoordinates = new Vector2(hex.x, hex.y);
      }
    }

    MapBounds mapBounds = new MapBounds();

    mapBounds.top = HexMathHelper.hexToWorldCoords(new Vector2(0, maxY + 2), hexSize).y;
    mapBounds.left = HexMathHelper.hexToWorldCoords(new Vector2(leftMostHexCoordinates.x - 2, leftMostHexCoordinates.y), hexSize).x;
    mapBounds.bottom = HexMathHelper.hexToWorldCoords(new Vector2(0, minY - 2), hexSize).y;
    mapBounds.right = HexMathHelper.hexToWorldCoords(new Vector2(rightMostHexCoordinates.x + 2, rightMostHexCoordinates.y), hexSize).x;

    return mapBounds;
  }

  private List<List<Vector2>> generateRiverPoints() {
    List<List<Vector2>> rivers = RiverHelper.generateRivers(map);

    return RiverHelper.getRiverPoints(rivers, riverDensity, hexSize, map);
  }

  public PolygonCollider2D generateRiversCollider(List<List<Vector2>> riverPoints) {
    PolygonCollider2D riverCollider = rivers.AddComponent<PolygonCollider2D>();
    riverCollider.enabled = false;
    riverCollider.pathCount = riverPoints.Count;
    for (int i = 0; i < riverPoints.Count; i++) {
      riverCollider.SetPath(i, RiverHelper.getRiverContour(riverPoints[i]).ToArray());
    }
    riverCollider.enabled = true;

    return riverCollider;
  }

  private int[] simplestTriangles(int size) {
    int[] triangles = new int[size];
    for (int i = 0; i < triangles.Length; i++) {
      triangles[i] = i;
    }

    return triangles;
  }

  private Color32[] simplestColours(Color color, int size) {
    Color32[] colours = new Color32[size];
    for (int i = 0; i < colours.Length; i++) {
      colours[i] = color;
    }

    return colours;
  }
}