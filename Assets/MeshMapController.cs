﻿using System.Collections;
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

  public GameObject ocean;
  public GameObject rivers;
  public GameObject lakes;

  public GameObject treePrefab;

  public System.Random randomGenerator;

  public int specifiedMapSeed = 0;

  public Map map;

  private UnityEngine.Mesh mesh;
  private List<Vector3> vertices;
  private List<int> triangles;
  private List<Color32> colours;
  private List<Vector2> uv;

  private Dictionary<Vector2, float> mapVerticesHeights = new Dictionary<Vector2, float>();

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

  private List<List<Vector2>> riverPoints = new List<List<Vector2>>();
  private float riverDensity = 0.15f;

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
    float heightSeed = Random.Range(0, 100);

    initGertsnerWaves();
    this.vertices = new List<Vector3>();
    this.triangles = new List<int>();
    this.colours = new List<Color32>();
    this.uv = new List<Vector2>();

    this.shoreIndices = new List<int>();
    this.initialHeights = new List<float>();

    List<Vector3> oceanVertices = new List<Vector3>();
    List<Vector3> riverVertices = new List<Vector3>();
    List<Vector3> lakeVertices = new List<Vector3>();

    calculateMapBounds();

    drawRivers(heightSeed);

    TriangleNet.Mesh mapTriangulation = triangulateMap();

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

      bool needsShoreAdjustment1 = false;
      bool needsShoreAdjustment2 = false;
      bool needsShoreAdjustment3 = false;

      Color resultTriangleColor = ColourHelper.WATER_COLOUR;

      if (triangleColor == triangleColor2 && triangleColor == triangleColor3) {
        resultTriangleColor = triangleColor;
      } else if (ColourHelper.isWaterColor(triangleColor) || ColourHelper.isWaterColor(triangleColor2) || ColourHelper.isWaterColor(triangleColor3)) {
        if (vertex1.hex == null || vertex2.hex == null || vertex3.hex == null) {
          resultTriangleColor = Color.yellow;
          if (vertex1.hex != null) {
            needsShoreAdjustment1 = true;
          }
          if (vertex2.hex != null) {
            needsShoreAdjustment2 = true;
          }
          if (vertex3.hex != null) {
            needsShoreAdjustment3 = true;
          }
        } else {
          resultTriangleColor = new Color32(70, 100, 0, 255);
        }

        Hex shoreHex = vertex1.hex != null && (vertex1.hex.biome == null || vertex1.hex.biome.name != "Fresh lake") ?
          vertex1.hex : vertex2.hex != null && (vertex2.hex.biome == null || vertex2.hex.biome.name != "Fresh lake") ? vertex2.hex : vertex3.hex;
        Vector2 shoreCenter = HexMathHelper.hexToWorldCoords(new Vector2(shoreHex.x, shoreHex.y), this.getHexSize());
        //if shore triangle push it underwater to create shore slope
        if (vertex1.hex == null) {
          underwaterShift1 = (new Vector2((float)vertex1.x, (float)vertex1.y) - shoreCenter).normalized * 0.3f;
          underwaterShift1 += new Vector3(0, 0, 0.03f);
          shoreIndices.Add(vertexIndex);
        }
        if (vertex2.hex == null) {
          underwaterShift2 = (new Vector2((float)vertex2.x, (float)vertex2.y) - shoreCenter).normalized * 0.3f;
          underwaterShift2 += new Vector3(0, 0, 0.03f);
          shoreIndices.Add(vertexIndex + 1);
        }
        if (vertex3.hex == null) {
          underwaterShift3 = (new Vector2((float)vertex3.x, (float)vertex3.y) - shoreCenter).normalized * 0.3f;
          underwaterShift3 += new Vector3(0, 0, 0.03f);
          shoreIndices.Add(vertexIndex + 2);
        }

        /*if (vertex1.hex != null && vertex1.hex.biome != null && vertex1.hex.biome.name == "Fresh lake") {
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
        }*/
      } else {
        Hex rarestHex = vertex1.hex;
        if (ColourHelper.biomeRarity(rarestHex) < ColourHelper.biomeRarity(vertex2.hex)) {
          rarestHex = vertex2.hex;
        }

        if (ColourHelper.biomeRarity(rarestHex) < ColourHelper.biomeRarity(vertex3.hex)) {
          rarestHex = vertex3.hex;
        }

        Color32 resultColour = ColourHelper.getHexColor(rarestHex);
        resultTriangleColor = resultColour;
      }

      float height1 = TerrainHelper.getHeight(heightSeed, vertex1, this.getHexSize(), elevatedHeightmap, mountainHeightmap);
      float height2 = TerrainHelper.getHeight(heightSeed, vertex2, this.getHexSize(), elevatedHeightmap, mountainHeightmap);
      float height3 = TerrainHelper.getHeight(heightSeed, vertex3, this.getHexSize(), elevatedHeightmap, mountainHeightmap);

      Vector3 newVertex1 = vertex1.toVector3() + underwaterShift1 + new Vector3(0, 0, height1);
      Vector3 newVertex2 = vertex2.toVector3() + underwaterShift2 + new Vector3(0, 0, height2);
      Vector3 newVertex3 = vertex3.toVector3() + underwaterShift3 + new Vector3(0, 0, height3);

      if(!this.mapVerticesHeights.ContainsKey(newVertex1) && needsShoreAdjustment1) {
        newVertex1.z = Random.Range(0.08f, 0.13f);
        this.mapVerticesHeights[newVertex1] = newVertex1.z;
      }

      if (!this.mapVerticesHeights.ContainsKey(newVertex2) && needsShoreAdjustment2) {
        newVertex2.z = Random.Range(0.08f, 0.13f);
        this.mapVerticesHeights[newVertex2] = newVertex2.z;
      }

      if (!this.mapVerticesHeights.ContainsKey(newVertex3) && needsShoreAdjustment3) {
        newVertex3.z = Random.Range(0.08f, 0.13f);
        this.mapVerticesHeights[newVertex3] = newVertex3.z;
      }

      if (vertex1.isRiver && (vertex1.hex == null || !vertex1.hex.hasTag(RiverHelper.FRESH_LAKE_TAG))) {
        newVertex1 += TerrainHelper.getRiverHeightAdjustment(newVertex1);
      }

      if (vertex2.isRiver && (vertex2.hex == null || !vertex2.hex.hasTag(RiverHelper.FRESH_LAKE_TAG))) {
        newVertex2 += TerrainHelper.getRiverHeightAdjustment(newVertex2);
      }

      if (vertex3.isRiver && (vertex3.hex == null || !vertex3.hex.hasTag(RiverHelper.FRESH_LAKE_TAG))) {
        newVertex3 += TerrainHelper.getRiverHeightAdjustment(newVertex3);
      }

      if (vertex1.hex == null && vertex2.hex == null && vertex3.hex == null) {
        oceanVertices.Add(newVertex1);
        oceanVertices.Add(newVertex2);
        oceanVertices.Add(newVertex3);
      } else if (vertex1.isRiver && vertex2.isRiver && vertex3.isRiver) {
        //riverVertices.Add(newVertex1);
        //riverVertices.Add(newVertex2);
        //riverVertices.Add(newVertex3);
        lakeVertices.Add(newVertex1);
        lakeVertices.Add(newVertex2);
        lakeVertices.Add(newVertex3);
      } else if (vertex1.hex != null && vertex1.hex.hasTag(RiverHelper.FRESH_LAKE_TAG) 
        && vertex2.hex != null && vertex2.hex.hasTag(RiverHelper.FRESH_LAKE_TAG)
        && vertex3.hex != null && vertex3.hex.hasTag(RiverHelper.FRESH_LAKE_TAG)) {
        lakeVertices.Add(newVertex1);
        lakeVertices.Add(newVertex2);
        lakeVertices.Add(newVertex3);
      } else {
        this.vertices.Add(newVertex1);
        this.vertices.Add(newVertex2);
        this.vertices.Add(newVertex3);

        float averageHeight = (newVertex1.z + newVertex2.z + newVertex3.z) / 3;
        if (averageHeight <= -0.4) {
          resultTriangleColor = Color.white;
        }

        this.colours.Add(resultTriangleColor);
        this.colours.Add(resultTriangleColor);
        this.colours.Add(resultTriangleColor);

        this.triangles.Add(vertexIndex);
        this.triangles.Add(vertexIndex + 1);
        this.triangles.Add(vertexIndex + 2);
      }
    }

    if (oceanVertices.Count > 0) {
      this.createOceanMesh(oceanVertices);
    }

    if(riverVertices.Count > 0) {
      this.createRiverMesh(riverVertices);
    }

    if (lakeVertices.Count > 0) {
      this.createLakesMesh(lakeVertices);
    }

    generateTrees(heightSeed);


    //smooth shore
    for (int i = 0; i < this.vertices.Count; i++) {
      Vector3 vertex = this.vertices[i];
      if (this.mapVerticesHeights.ContainsKey(vertex)) {
        vertex.z = this.mapVerticesHeights[vertices[i]];
        this.vertices[i] = vertex;
      }
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

  public void createRiverMesh (List<Vector3> vertices) {
    UnityEngine.Mesh riverMesh = new UnityEngine.Mesh();
    riverMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    riverMesh.name = "Rivers";
    riverMesh.vertices = vertices.ToArray();
    riverMesh.triangles = simplestTriangles(vertices.Count);
    riverMesh.colors32 = simplestColours(ColourHelper.WATER_COLOUR, vertices.Count);
    riverMesh.RecalculateNormals();
    riverMesh.RecalculateBounds();

    rivers.GetComponent<MeshFilter>().mesh = riverMesh;
  }

  public void generateTrees(float heightSeed) {
    foreach (Hex hex in this.map.hexes) {
      if (hex != null && hex.biome != null && hex.biome.name == "Grassland") {
        int forestThreshold = 30;
        int randomChance = Random.Range(0, 100);
        if(randomChance < forestThreshold) {
          int numberOfTrees = Random.Range(15, 30);
          for (int treeIndex = 0; treeIndex < numberOfTrees; treeIndex++) {
            spawnTree(hex, heightSeed);
          }
        }
      }
    }
  }

  public void spawnTree(Hex hex, float heightSeed) {
    GameObject newTree = Instantiate(treePrefab);
    Vector2 treePosition = Random.insideUnitCircle * this.getHexSize();
    treePosition += HexMathHelper.hexToWorldCoords(hex.getPosition(), this.getHexSize());
    float spawnHeight = TerrainHelper.getHeight(heightSeed, treePosition, hex, this.getHexSize(), elevatedHeightmap, mountainHeightmap);
    float yScale = Random.Range(0.05f, 0.08f);
    Vector3 newScale = newTree.transform.localScale;
    newScale.y = yScale;
    newTree.transform.localScale = newScale;
    newTree.transform.position = new Vector3(treePosition.x, treePosition.y, spawnHeight - 0.12f);
  }

  public void createLakesMesh(List<Vector3> vertices) {
    UnityEngine.Mesh lakesMesh = new UnityEngine.Mesh();
    lakesMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    lakesMesh.name = "Lakes";
    lakesMesh.vertices = vertices.ToArray();
    lakesMesh.triangles = simplestTriangles(vertices.Count);
    lakesMesh.colors32 = simplestColours(ColourHelper.WATER_COLOUR, vertices.Count);
    lakesMesh.RecalculateNormals();
    lakesMesh.RecalculateBounds();

    lakes.GetComponent<MeshFilter>().mesh = lakesMesh;
  }

  public void createOceanMesh(List<Vector3> vertices) {
    UnityEngine.Mesh oceanMesh = new UnityEngine.Mesh();
    oceanMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    oceanMesh.name = "Ocean";
    oceanMesh.vertices = vertices.ToArray();
    oceanMesh.triangles = simplestTriangles(vertices.Count);
    oceanMesh.colors32 = simplestColours(ColourHelper.WATER_COLOUR, vertices.Count);
    oceanMesh.RecalculateNormals();
    oceanMesh.RecalculateBounds();

    ocean.GetComponent<MeshFilter>().mesh = oceanMesh;
  }

  void drawRivers(float heightSeed) {
    List<List<Vector2>> rivers = RiverHelper.generateRivers(map, this.mapCenter);
    this.riverPoints = RiverHelper.getRiverPoints(rivers, this.riverDensity, this.getHexSize(), this.map);
    /*this.rivers.GetComponent<MeshFilter>().mesh = 
      RiverHelper.getRiversMesh(rivers, this.riverDensity, this.getHexSize(), heightSeed, 
        mountainHeightmap, elevatedHeightmap, this.map);*/
  }

  private Vector2 getHexRiverPoint(Hex hex, float hexSize) {
    Vector2 hexCenter = HexMathHelper.hexToWorldCoords(new Vector2(hex.x, hex.y), hexSize);

    return new Vector2(hexCenter.x + hex.riverPoint.x * hexSize, hexCenter.y + hex.riverPoint.y * hexSize);
  }

  public void OnDrawGizmos() {
    Gizmos.color = Color.red;
    foreach (List<Vector2> river in this.riverPoints) {
      foreach(Vector2 riverPoint in river) {
        Gizmos.DrawSphere(new Vector3(riverPoint.x, riverPoint.y, -1), 0.01f);
      }
    }
  }

  public TriangleNet.Mesh triangulateMap() {
    PolygonCollider2D riverCollider = rivers.AddComponent<PolygonCollider2D>();
    riverCollider.enabled = false;
    riverCollider.pathCount = this.riverPoints.Count;
    for (int i = 0; i < this.riverPoints.Count; i++) {
      riverCollider.SetPath(i, RiverHelper.getRiverContour(this.riverPoints[i]).ToArray());
    }
    riverCollider.enabled = true;

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
        if (!riverCollider.OverlapPoint(new Vector2(x, y + yDispersion))) {
          polygon.Add(new MapVertex(x, y + yDispersion, vertexHex));
        }
      }
      if (y == top) {
        break;
      }
      y += polygonDensity * (1 + Random.Range(-0.3f, 0.3f));
      if (y > top) {
        y = top;
      }
    }

    foreach (List<Vector2> river in this.riverPoints) {
      foreach (Vector2 riverPoint in river) {
        Hex vertexHex = map.getHex(HexMathHelper.worldToHexCoords(riverPoint, this.getHexSize()));
        polygon.Add(new MapVertex(riverPoint.x, riverPoint.y, vertexHex, true));
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
