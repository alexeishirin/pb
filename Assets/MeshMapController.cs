using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Topology;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshMapController : MonoBehaviour {
  public MapGenerationSettings mapGenerationSettings;
  public MapData mapData;
  public Texture2D mountainHeightmap;
  public Texture2D elevatedHeightmap;

  public System.Random randomGenerator;

  public int specifiedMapSeed = 0;

  public Map map;

  private UnityEngine.Mesh mesh;
  private List<Vector3> vertices;
  private List<int> triangles;
  private List<Color32> colours;
  private List<Vector2> uv;

  private int minX;
  private int maxX;
  private int minY;
  private int maxY;
  private float left;
  private float right;
  private float top;
  private float bottom;
  private Vector2 mapCenter;
  private Vector2 leftMostHexCoordinates;
  private Vector2 rightMostHexCoordinates;
  private HashSet<Vector3> shoreVertices = new HashSet<Vector3>();
  private List<int> shoreIndices = new List<int>();
  private List<float> initialHeights = new List<float>();
  private List<Vector2> holes = new List<Vector2>();

  private Vector3[] initialVertices;
  private List<GertsnerWave> gertsnerWaves;

  private float polygonDensity = 0.2f * 1f;

  private List<Vector3> riverPoints = new List<Vector3>();
  private float riverDensity = 0.05f;

  void Start() {
  }

  void Update() {
    if (mesh != null) {
      //animateWater();
      //mesh.RecalculateNormals();
    }
  }

  public void animateWater() {
    Vector3[] vertices = mesh.vertices;
    Color32[] colours = mesh.colors32;
    for (int i = 0; i < vertices.Length; i++) {
      if (colours[i].r == 0 && colours[i].g == 0 && colours[i].b == 255) {
        vertices[i] = new Vector3(initialVertices[i].x, initialVertices[i].y, 0.25f);
        foreach (GertsnerWave gertsnerWave in this.gertsnerWaves) {
          vertices[i] += gertsnerWave.getPointShift(initialVertices[i], Time.timeSinceLevelLoad);
        }
        //vertices[i].z = 0.25f + 0.1f / 3f * (Mathf.Sin(Time.timeSinceLevelLoad + vertices[i].x) * Mathf.Sin(Time.timeSinceLevelLoad + vertices[i].x) + Mathf.Sin(Time.timeSinceLevelLoad + vertices[i].x) * Mathf.Cos(Time.timeSinceLevelLoad + vertices[i].y) + Mathf.Cos(Time.timeSinceLevelLoad + vertices[i].x) * Mathf.Sin(Time.timeSinceLevelLoad + vertices[i].y));
      }
    }
    foreach (int verticeIndex in this.shoreIndices) {
      vertices[verticeIndex].z = this.initialHeights[verticeIndex] + 0.2f;
    }
    mesh.vertices = vertices;
  }

  public void regenerateMap(Map newMap) {
    clearMesh();
    this.map = newMap;
    drawMap();
  }

  public void initGertsnerWaves() {
    this.gertsnerWaves = new List<GertsnerWave>();
    this.gertsnerWaves.Add(new GertsnerWave(0.04f, 1.0f, 0.1f, new Vector2(-1, 0)));
    //this.gertsnerWaves.Add(new GertsnerWave(0.03f, 0.4f, 0.2f, new Vector2(-0.2f, 0.8f)));
    //this.gertsnerWaves.Add(new GertsnerWave(0.05f, 0.6f, 0.09f, new Vector2(0.5f, -0.5f)));
    //this.gertsnerWaves.Add(new GertsnerWave(0.04f, 0.7f, 0.07f, new Vector2(-0.3f, -0.7f)));
  }
  public void clearMesh() {
    this.vertices = new List<Vector3>();
    this.triangles = new List<int>();
    this.colours = new List<Color32>();
    this.uv = new List<Vector2>();
    this.mesh.vertices = vertices.ToArray();
    this.mesh.triangles = triangles.ToArray();
    this.mesh.colors32 = colours.ToArray();
    this.mesh.uv = new List<Vector2>().ToArray();
  }

  public void clearMap() {
    this.map = null;
  }

  public void setMapSeed(int newSeed) {
    this.specifiedMapSeed = newSeed;
  }

  public void drawMap() {
    initGertsnerWaves();
    drawRivers();
    this.vertices = new List<Vector3>();
    this.triangles = new List<int>();
    this.colours = new List<Color32>();
    this.uv = new List<Vector2>();

    this.shoreIndices = new List<int>();
    this.initialHeights = new List<float>();

    calculateMapBounds();

    TriangleNet.Mesh mapTriangulation = triangulateMap();
    float heightSeed = Random.Range(0, 100);

    foreach (Triangle triangle in mapTriangulation.Triangles) {
      int vertexIndex = this.vertices.Count;
      MapVertex vertex1 = triangle.vertices[2] is MapVertex ? (MapVertex)triangle.vertices[2] : convertToMapVertex(triangle.vertices[2]);
      MapVertex vertex2 = triangle.vertices[1] is MapVertex ? (MapVertex)triangle.vertices[1] : convertToMapVertex(triangle.vertices[1]);
      MapVertex vertex3 = triangle.vertices[0] is MapVertex ? (MapVertex)triangle.vertices[0] : convertToMapVertex(triangle.vertices[0]);

      Color triangleColor = ColourHelper.getHexColor(vertex1.hex);
      Color triangleColor2 = ColourHelper.getHexColor(vertex2.hex);
      Color triangleColor3 = ColourHelper.getHexColor(vertex3.hex);

      Vector3 underwaterShift1 = new Vector3(0, 0);
      Vector3 underwaterShift2 = new Vector3(0, 0);
      Vector3 underwaterShift3 = new Vector3(0, 0);

      if (triangleColor == triangleColor2 && triangleColor == triangleColor3) {
        this.colours.Add(triangleColor);
        this.colours.Add(triangleColor);
        this.colours.Add(triangleColor);
      } else if (ColourHelper.isWaterColor(triangleColor) || ColourHelper.isWaterColor(triangleColor2) || ColourHelper.isWaterColor(triangleColor3)) {
        this.colours.Add(Color.yellow);
        this.colours.Add(Color.yellow);
        this.colours.Add(Color.yellow);

        Hex shoreHex = vertex1.hex != null && (vertex1.hex.biome == null || vertex1.hex.biome.name != "Fresh lake") ?
          vertex1.hex : vertex2.hex != null && (vertex2.hex.biome == null || vertex2.hex.biome.name != "Fresh lake") ? vertex2.hex : vertex3.hex;
        Vector2 shoreCenter = HexMathHelper.hexToWorldCoords(new Vector2(shoreHex.x, shoreHex.y), this.getHexSize());
        //if shore triangle push it underwater to create shore slope
        if (vertex1.hex == null) {
          underwaterShift1 = (new Vector2((float)vertex1.x, (float)vertex1.y) - shoreCenter).normalized * 0.3f;
          shoreIndices.Add(vertexIndex);
        }
        if (vertex2.hex == null) {
          underwaterShift2 = (new Vector2((float)vertex2.x, (float)vertex2.y) - shoreCenter).normalized * 0.3f;
          shoreIndices.Add(vertexIndex + 1);
        }
        if (vertex3.hex == null) {
          underwaterShift3 = (new Vector2((float)vertex3.x, (float)vertex3.y) - shoreCenter).normalized * 0.3f;
          shoreIndices.Add(vertexIndex + 2);
        }

        if (vertex1.hex != null && vertex1.hex.biome != null && vertex1.hex.biome.name == "Fresh lake") {
          underwaterShift1 = (new Vector2((float)vertex1.x, (float)vertex1.y) - shoreCenter).normalized * 0.1f;
          shoreIndices.Add(vertexIndex);
        }
        if (vertex2.hex != null && vertex2.hex.biome != null && vertex2.hex.biome.name == "Fresh lake") {
          underwaterShift2 = (new Vector2((float)vertex2.x, (float)vertex2.y) - shoreCenter).normalized * 0.1f;
          shoreIndices.Add(vertexIndex + 1);
        }
        if (vertex3.hex != null && vertex3.hex.biome != null && vertex3.hex.biome.name == "Fresh lake") {
          underwaterShift3 = (new Vector2((float)vertex3.x, (float)vertex3.y) - shoreCenter).normalized * 0.1f;
          shoreIndices.Add(vertexIndex + 2);
        }
      } else {
        Hex rarestHex = vertex1.hex;
        if (ColourHelper.biomeRarity(rarestHex) < ColourHelper.biomeRarity(vertex2.hex)) {
          rarestHex = vertex2.hex;
        }

        if (ColourHelper.biomeRarity(rarestHex) < ColourHelper.biomeRarity(vertex3.hex)) {
          rarestHex = vertex3.hex;
        }

        Color32 resultColour = ColourHelper.getHexColor(rarestHex);
        this.colours.Add(resultColour);
        this.colours.Add(resultColour);
        this.colours.Add(resultColour);
      }

      float height1 = TerrainHelper.getHeight(heightSeed, vertex1.toVector3(), vertex1.hex, this.getHexSize(), elevatedHeightmap, mountainHeightmap);
      float height2 = TerrainHelper.getHeight(heightSeed, vertex2.toVector3(), vertex2.hex, this.getHexSize(), elevatedHeightmap, mountainHeightmap);
      float height3 = TerrainHelper.getHeight(heightSeed, vertex3.toVector3(), vertex3.hex, this.getHexSize(), elevatedHeightmap, mountainHeightmap);

      this.vertices.Add(vertex1.toVector3() + underwaterShift1 + new Vector3(0, 0, height1));
      this.vertices.Add(vertex2.toVector3() + underwaterShift2 + new Vector3(0, 0, height2));
      this.vertices.Add(vertex3.toVector3() + underwaterShift3 + new Vector3(0, 0, height3));

      this.initialHeights.Add(height1);
      this.initialHeights.Add(height2);
      this.initialHeights.Add(height3);

      this.triangles.Add(vertexIndex);
      this.triangles.Add(vertexIndex + 1);
      this.triangles.Add(vertexIndex + 2);
    }


    GetComponent<MeshFilter>().mesh = mesh = new UnityEngine.Mesh();
    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    mesh.name = "Procedural Grid";
    mesh.vertices = this.vertices.ToArray();
    mesh.triangles = this.triangles.ToArray();
    mesh.colors32 = this.colours.ToArray();
    mesh.RecalculateNormals();
    mesh.RecalculateBounds();
    this.initialVertices = this.vertices.ToArray();
  }

  void drawRivers() {
    List<List<Hex>> rivers = RiverHelper.generateRivers(map);
    Debug.Log(rivers.Count);
    this.riverPoints = RiverHelper.getRiversMesh(rivers, this.riverDensity, this.getHexSize());
    Debug.Log(this.riverPoints.Count);
  }

  private Vector2 getHexRiverPoint(Hex hex, float hexSize) {
    Vector2 hexCenter = HexMathHelper.hexToWorldCoords(new Vector2(hex.x, hex.y), hexSize);

    return new Vector2(hexCenter.x + hex.riverPoint.x * hexSize, hexCenter.y + hex.riverPoint.y * hexSize);
  }

  public void OnDrawGizmos() {
    Gizmos.color = Color.red;
    for (int i = 0; i < this.riverPoints.Count; i++) {
      Gizmos.DrawSphere(this.riverPoints[i], 0.01f);
      //Gizmos.DrawSphere(this.riverPoints[i + 1], 0.01f);
      //Gizmos.DrawLine(this.riverPoints[i], this.riverPoints[i + 1]);
    }
    Gizmos.color = Color.green;
    Gizmos.DrawSphere(this.riverPoints[0], 0.01f);

    Gizmos.color = Color.blue;
    Gizmos.DrawSphere(this.riverPoints[this.riverPoints.Count - 1], 0.01f);
  }

  public TriangleNet.Mesh triangulateMap() {
    Polygon polygon = new Polygon();

    polygon.Add(new Vertex(left, bottom));
    for (float y = bottom; y <= top;) {
      Hex rowLeftHex = map.getHex(HexMathHelper.worldToHexCoords(new Vector2(left, y), this.getHexSize()));
      polygon.Add(new MapVertex(left, y, rowLeftHex));
      for (float x = left; x < right;) {
        x += polygonDensity * (1 + Random.Range(-0.4f, 0.4f));
        if (x > right) {
          x = right;
        }
        float yDispersion = y == bottom || y == top ? 0 : polygonDensity * Random.Range(-0.3f, 0.3f);
        Hex vertexHex = map.getHex(HexMathHelper.worldToHexCoords(new Vector2(x, y + yDispersion), this.getHexSize()));
        polygon.Add(new MapVertex(x, y + yDispersion, vertexHex));
      }
      if (y == top) {
        break;
      }
      y += polygonDensity * (1 + Random.Range(-0.3f, 0.3f));
      if (y > top) {
        y = top;
      }
    }

    TriangleNet.Meshing.ConstraintOptions options =
    new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };
    return (TriangleNet.Mesh)polygon.Triangulate(options);
  }

  public MapVertex convertToMapVertex(Vertex vertex) {
    Hex vertexHex = map.getHex(HexMathHelper.worldToHexCoords(new Vector2((float)vertex.x, (float)vertex.y), this.getHexSize()));

    return new MapVertex(vertex.x, vertex.y, vertexHex);
  }

  public Vector2 getFixedCoordinates(Hex hex) {
    return new Vector2(hex.x - minX, hex.y - minY);
  }


  private int[] simplestTriangles(Vector3[] vertices) {
    int[] triangles = new int[vertices.Length];
    for (int i = 0; i < triangles.Length; i++) {
      triangles[i] = i;
    }

    return triangles;
  }

  public float getHexSize() {
    return 1;
  }

  private void calculateMapBounds() {
    minX = map.hexes[0].x;
    maxX = map.hexes[0].x;
    minY = map.hexes[0].y;
    maxY = map.hexes[0].y;
    leftMostHexCoordinates = new Vector2(map.hexes[0].x, map.hexes[0].y);
    rightMostHexCoordinates = new Vector2(map.hexes[0].x, map.hexes[0].y);
    foreach (Hex hex in map.hexes) {
      if (hex.x > maxX) {
        maxX = hex.x;
      }
      if (hex.x < minX) {
        minX = hex.x;
      }
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

    this.top = HexMathHelper.hexToWorldCoords(new Vector2(minX - 2, maxY + 2), this.getHexSize()).y;
    this.left = HexMathHelper.hexToWorldCoords(new Vector2(leftMostHexCoordinates.x - 2, leftMostHexCoordinates.y), this.getHexSize()).x;
    this.bottom = HexMathHelper.hexToWorldCoords(new Vector2(maxX + 2, minY - 2), this.getHexSize()).y;
    this.right = HexMathHelper.hexToWorldCoords(new Vector2(rightMostHexCoordinates.x + 2, rightMostHexCoordinates.y), this.getHexSize()).x;
    float mapHeight = top - bottom;
    float mapWidth = right - left;
    this.mapCenter = new Vector2(right - mapWidth / 2, top - mapHeight / 2);
  }

  public static MeshMapController getInstance() {
    return GameObject.FindGameObjectWithTag("Map").GetComponent<MeshMapController>();
  }

}
