using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Topology;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshMapBakup : MonoBehaviour {
  public MapGenerationSettings mapGenerationSettings;
  public MapData mapData;
  public Texture2D mountainHeightmap;
  public Texture2D elevatedHeightmap;

  public System.Random randomGenerator;

  public int specifiedMapSeed = 0;

  public Map map;


  private UnityEngine.Mesh mesh;
  private TriangleNet.Mesh testMesh;
  private List<Vector3> vertices;
  private List<int> triangles;
  private List<Color32> colours;
  private List<Vector2> uv;

  private Vector2[,] hexCornersDisplacements;
  private int[,] regionIds;
  private float[,] surfaceCurving;
  private int minX;
  private int maxX;
  private int minY;
  private int maxY;
  private float left;
  private float right;
  private float top;
  private float bottom;
  private Vector3 mapCenter;
  private Vector2 leftMostHexCoordinates;
  private Vector2 rightMostHexCoordinates;
  private HashSet<Vector3> shoreVertices = new HashSet<Vector3>();
  private HashSet<Vertex> underWaterContour = new HashSet<Vertex>();
  private HashSet<Vertex> shoreContour = new HashSet<Vertex>();
  private HashSet<int> shoreIndices = new HashSet<int>();
  private List<Vector2> holes = new List<Vector2>();

  private float polygonDensity = 0.2f * 1f;
  public int xVisibleHexes = 20;
  public int yVisibleHexes = 20;

  private List<Vector2> hexCornerVectors;

  // Use this for initialization
  void Start() {
    //MapGenerator.generateMap();
    //drawMap();
  }

  // Update is called once per frame
  void Update() {
    if (mesh != null) {
      moveVertices();
      mesh.RecalculateNormals();
    }
  }

  public void moveVertices() {
    Vector3[] vertices = mesh.vertices;
    Color32[] colours = mesh.colors32;
    for (int i = 0; i < vertices.Length; i++) {
      if (colours[i].r == 0 && colours[i].g == 0 && colours[i].b == 255) {
        //vertices[i].z += Random.Range(-0.02f, 0.02f);
        vertices[i].z = 0.35f + 0.1f * Mathf.Sin(Time.timeSinceLevelLoad + vertices[i].x + vertices[i].y);
        //vertices[i].z = Mathf.Clamp(vertices[i].z, -0.2f, 0.2f);
      }
    }
    foreach (int verticeIndex in this.shoreIndices) {
      vertices[verticeIndex].z = 0.45f;
    }
    mesh.vertices = vertices;

  }

  public void regenerateMap(Map newMap) {
    clearMesh();
    this.map = newMap;
    drawMap();
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

  public Region getRegionById(int id) {
    foreach (Region region in map.regions) {
      if (region.id == id) {
        return region;
      }
    }

    return null;
  }

  public void clearMap() {
    this.map = null;
  }

  public void setMapSeed(int newSeed) {
    this.specifiedMapSeed = newSeed;
  }

  public void drawMap() {
    this.vertices = new List<Vector3>();
    this.triangles = new List<int>();
    this.colours = new List<Color32>();
    this.uv = new List<Vector2>();
    this.hexCornerVectors = HexMathHelper.getHexCorners(this.getHexSize());
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
    regionIds = new int[maxX - minX + 1, maxY - minY + 1];
    foreach (Hex hex in map.hexes) {
      regionIds[hex.x - minX, hex.y - minY] = hex.regionId;
    }

    hexCornersDisplacements = new Vector2[(maxX - minX + 1) * 2 + 2, maxY - minY + 2];
    smoothShores(map.hexes);

    // foreach (Hex hex in map.hexes) {
    //   drawHex(hex);
    // }

    generateTerrain();
    foreach (Triangle triangle in testMesh.Triangles) {
      int vertexIndex = this.vertices.Count;
      this.vertices.Add(new Vector3((float)triangle.vertices[2].x, (float)triangle.vertices[2].y));
      this.vertices.Add(new Vector3((float)triangle.vertices[1].x, (float)triangle.vertices[1].y));
      this.vertices.Add(new Vector3((float)triangle.vertices[0].x, (float)triangle.vertices[0].y));

      this.triangles.Add(vertexIndex);
      this.triangles.Add(vertexIndex + 1);
      this.triangles.Add(vertexIndex + 2);
    }


    Vector2 cameraCenter = UIController.getInstance().mainCamera.transform.position;
    Debug.Log(cameraCenter);
    List<Vector3> visibleVertices = new List<Vector3>();

    for (int i = 0; i <= this.vertices.Count - 3; i += 3) {
      //if(isVertexVisible(cameraCenter, this.vertices[i]) 
      //&& isVertexVisible(cameraCenter, this.vertices[i + 1]) 
      // && isVertexVisible(cameraCenter, this.vertices[i + 2])) {

      Hex hex1 = map.getHex(HexMathHelper.worldToHexCoords(this.vertices[i], this.getHexSize()));
      Hex hex2 = map.getHex(HexMathHelper.worldToHexCoords(this.vertices[i + 1], this.getHexSize()));
      Hex hex3 = map.getHex(HexMathHelper.worldToHexCoords(this.vertices[i + 2], this.getHexSize()));
      bool shoreBound = hex1 == null || hex2 == null || hex3 == null;
      Color triangleColor = this.getHexColor(hex1);
      Color triangleColor2 = this.getHexColor(hex2);
      Color triangleColor3 = this.getHexColor(hex3);
      if (triangleColor == triangleColor2 && triangleColor == triangleColor3) {
        this.colours.Add(triangleColor);
        this.colours.Add(triangleColor);
        this.colours.Add(triangleColor);
      } else {
        bool isFirstVerticeWater = isWaterColor(triangleColor);
        bool isSecondVerticeWater = isWaterColor(triangleColor2);
        bool isThirdVerticeWater = isWaterColor(triangleColor3);
        if (((isFirstVerticeWater ? 1 : 0) + (isSecondVerticeWater ? 1 : 0) + (isThirdVerticeWater ? 1 : 0)) == 1) {
          int pointIndex = isFirstVerticeWater ? i : isSecondVerticeWater ? i + 1 : i + 2;
          int secondVerticeIndex = isFirstVerticeWater ? i + 1 : isSecondVerticeWater ? i + 2 : i;
          int thirdVerticeIndex = isFirstVerticeWater ? i + 2 : isSecondVerticeWater ? i : i + 1;
          shoreVertices.Add(this.vertices[pointIndex]);
          shoreIndices.Add(pointIndex);
          Vector3 toWaterVerticeVector = this.vertices[pointIndex] - this.vertices[secondVerticeIndex];
          Vector3 projection = Vector3.Project(toWaterVerticeVector, this.vertices[thirdVerticeIndex] - this.vertices[secondVerticeIndex]);
          Vector3 heightVector = toWaterVerticeVector - projection;
          shoreVertices.Add(this.vertices[pointIndex] + heightVector);

          if (shoreBound) {
            shoreIndices.Add(pointIndex);
            this.vertices[pointIndex] = this.vertices[pointIndex] + (this.vertices[pointIndex] - this.mapCenter).normalized * 0.5f;
            shoreContour.Add(new Vertex(this.vertices[pointIndex].x, this.vertices[pointIndex].y));
            underWaterContour.Add(new Vertex((this.vertices[pointIndex] + heightVector).x, (this.vertices[pointIndex] + heightVector).y));
          }
          //this.vertices[pointIndex] = this.vertices[pointIndex] + heightVector;
        } else if (((isFirstVerticeWater ? 1 : 0) + (isSecondVerticeWater ? 1 : 0) + (isThirdVerticeWater ? 1 : 0)) == 2) {
          int shoreIndex = !isFirstVerticeWater ? i : !isSecondVerticeWater ? i + 1 : i + 2;
          int secondVerticeIndex = !isFirstVerticeWater ? i + 1 : !isSecondVerticeWater ? i + 2 : i;
          int thirdVerticeIndex = !isFirstVerticeWater ? i + 2 : !isSecondVerticeWater ? i : i + 1;
          shoreVertices.Add(this.vertices[secondVerticeIndex]);

          shoreVertices.Add(this.vertices[secondVerticeIndex] + (this.vertices[secondVerticeIndex] - this.vertices[shoreIndex]));

          //this.vertices[secondVerticeIndex] = this.vertices[secondVerticeIndex] + (this.vertices[secondVerticeIndex] - this.vertices[shoreIndex]) + shoreHeightShift;
          shoreVertices.Add(this.vertices[thirdVerticeIndex]);

          shoreVertices.Add(this.vertices[thirdVerticeIndex] + (this.vertices[thirdVerticeIndex] - this.vertices[shoreIndex]));

          //this.vertices[thirdVerticeIndex] = this.vertices[thirdVerticeIndex] + (this.vertices[thirdVerticeIndex] - this.vertices[shoreIndex]) + shoreHeightShift;
          if (shoreBound) {
            shoreIndices.Add(secondVerticeIndex);
            this.vertices[secondVerticeIndex] = this.vertices[secondVerticeIndex] + (this.vertices[secondVerticeIndex] - this.mapCenter).normalized * 0.5f;
            shoreContour.Add(new Vertex(this.vertices[secondVerticeIndex].x, this.vertices[secondVerticeIndex].y));
            underWaterContour.Add(new Vertex((this.vertices[secondVerticeIndex] + (this.vertices[secondVerticeIndex] - this.vertices[shoreIndex])).x, (this.vertices[secondVerticeIndex] + (this.vertices[secondVerticeIndex] - this.vertices[shoreIndex])).y));
            shoreIndices.Add(thirdVerticeIndex);
            this.vertices[thirdVerticeIndex] = this.vertices[thirdVerticeIndex] + (this.vertices[thirdVerticeIndex] - this.mapCenter).normalized * 0.5f;
            shoreContour.Add(new Vertex(this.vertices[thirdVerticeIndex].x, this.vertices[thirdVerticeIndex].y));
            underWaterContour.Add(new Vertex((this.vertices[thirdVerticeIndex] + (this.vertices[thirdVerticeIndex] - this.vertices[shoreIndex])).x, (this.vertices[thirdVerticeIndex] + (this.vertices[thirdVerticeIndex] - this.vertices[shoreIndex])).y));
          }
        }

        if (isFirstVerticeWater || isSecondVerticeWater || isThirdVerticeWater) {
          this.colours.Add(Color.yellow);
          this.colours.Add(Color.yellow);
          this.colours.Add(Color.yellow);
        } else {
          this.colours.Add(new Color32(255, 255, 255, 255));
          this.colours.Add(new Color32(255, 255, 255, 255));
          this.colours.Add(new Color32(255, 255, 255, 255));
        }
      }

      this.vertices[i] = new Vector3(this.vertices[i].x, this.vertices[i].y, this.getHeight(this.vertices[i], hex1));
      this.vertices[i + 1] = new Vector3(this.vertices[i + 1].x, this.vertices[i + 1].y, this.getHeight(this.vertices[i + 1], hex2));
      this.vertices[i + 2] = new Vector3(this.vertices[i + 2].x, this.vertices[i + 2].y, this.getHeight(this.vertices[i + 2], hex3));

      visibleVertices.Add(this.vertices[i]);
      visibleVertices.Add(this.vertices[i + 1]);
      visibleVertices.Add(this.vertices[i + 2]);
      //}
    }
    Debug.Log(visibleVertices.Count);
    Debug.Log(underWaterContour.Count);
    Debug.Log(shoreContour.Count);
    /*Contour underwaterCon = new Contour(underWaterContour);
    Contour shoreCon = new Contour(shoreContour);
    Polygon polygon = new Polygon();
    polygon.Add(underwaterCon, false);
    polygon.Add(shoreCon, true);
    TriangleNet.Meshing.ConstraintOptions options =
    new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };
    testMesh = (TriangleNet.Mesh)polygon.Triangulate(options);*/

    GetComponent<MeshFilter>().mesh = mesh = new UnityEngine.Mesh();
    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    mesh.name = "Procedural Grid";
    mesh.vertices = visibleVertices.ToArray();
    mesh.triangles = simplestTriangles(visibleVertices.ToArray());
    mesh.colors32 = this.colours.ToArray();
    //mesh.uv = this.uv.ToArray();
    //Vector4[] tangents = new Vector4[mesh.vertices.Length];
    //Vector4 right = new Vector4(1f, 0f, 0f, -1f);
    //for (int i = 0; i < mesh.vertices.Length; i++) {
    //  tangents[i] = right;
    //}
    //mesh.tangents = tangents;
    mesh.RecalculateNormals();
    //mesh.RecalculateTangents();
    mesh.RecalculateBounds();
  }

  public int getHexColourWeight(Hex hex) {
    if (hex == null) {
      return -1;
    }

    return hex.x - minX + hex.y - minY;
  }

  public bool isWaterColor(Color32 color) {
    return color.r == 0 && color.g == 0 && color.b == 255;
  }

  public float getHeight(Vector3 vertex, Hex hex) {
    if (hex == null) {
      //water
      return 0.2f + getTerrainCurvingHeight(vertex);
    }
    Vector2 hexCenter = HexMathHelper.hexToWorldCoords(new Vector2(hex.x, hex.y), this.getHexSize());
    Vector2 hexShift = hexCenter - new Vector2(vertex.x, vertex.y);
    Vector2 heightMapCoordinate = new Vector2(0.5f + hexShift.x / this.getHexSize(), 0.5f + hexShift.y / this.getHexSize());

    float terrainCurvingHeight = getTerrainCurvingHeight(vertex);
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

  public static Color combineColors(List<Color> colors) {
    Color result = new Color(0, 0, 0, 0);
    foreach (Color color in colors) {
      result += color;
    }
    result = result / colors.Count;

    return result;
  }

  public bool isVertexVisible(Vector2 cameraCenter, Vector3 vertex) {
    return
      vertex.x >= cameraCenter.x - xVisibleHexes * this.getHexSize() * 0.5f
      && vertex.x <= cameraCenter.x + xVisibleHexes * this.getHexSize() * 0.5f
      && vertex.y >= cameraCenter.y - yVisibleHexes * this.getHexSize() * 0.5f
      && vertex.y <= cameraCenter.y + yVisibleHexes * this.getHexSize() * 0.5f;
  }

  public float[,] generateSurfaceCurving(float top, float left, float bottom, float right) {
    top = Mathf.Ceil(top);
    left = Mathf.Floor(left);
    bottom = Mathf.Floor(bottom);
    right = Mathf.Ceil(right);

    float[,] surfaceCurving = new float[(int)(right - left + 1), (int)(top - bottom + 1)];
    for (int x = 0; x <= (int)(right - left); x++) {
      for (int y = 0; y <= (int)(top - bottom); y++) {
        surfaceCurving[x, y] = Random.Range(-0.2f, 0.2f);
      }
    }

    return surfaceCurving;
  }

  public void generateTerrain() {
    this.holes.Add(new Vector2(2, 2));
    this.top = HexMathHelper.hexToWorldCoords(new Vector2(minX - 2, maxY + 2), this.getHexSize()).y;
    this.left = HexMathHelper.hexToWorldCoords(new Vector2(leftMostHexCoordinates.x - 2, leftMostHexCoordinates.y), this.getHexSize()).x;
    this.bottom = HexMathHelper.hexToWorldCoords(new Vector2(maxX + 2, minY - 2), this.getHexSize()).y;
    this.right = HexMathHelper.hexToWorldCoords(new Vector2(rightMostHexCoordinates.x + 2, rightMostHexCoordinates.y), this.getHexSize()).x;
    float mapHeight = top - bottom;
    float mapWidth = right - left;
    this.mapCenter = new Vector2(right - mapWidth / 2, top - mapHeight / 2);

    this.surfaceCurving = generateSurfaceCurving(top, left, bottom, right);
    Polygon polygon = new Polygon();
    //polygon.Add(new Vertex(left, top));
    //polygon.Add(new Vertex(right, top));
    //polygon.Add(new Vertex(right, bottom));
    //polygon.Add(new Vertex(left, bottom));

    for (float y = bottom; y <= top;) {
      for (float x = left; x < right;) {
        x += polygonDensity * (1 + Random.Range(-0.4f, 0.4f));
        if (x > right) {
          x = right;
        }
        float yDispersion = y == bottom || y == top ? 0 : polygonDensity * Random.Range(-0.3f, 0.3f);
        if (!this.holes.Exists(hole => hole == HexMathHelper.worldToHexCoords(new Vector2(x, y + yDispersion), this.getHexSize()))) {
          polygon.Add(new Vertex(x, y + yDispersion));
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

    foreach (Vector2 hole in holes) {
      Vector2 hexCenter = HexMathHelper.hexToWorldCoords(hole, this.getHexSize());

      Vector2 topCorner = hexCenter + this.hexCornerVectors[0];
      Vector2 rightTopCorner = hexCenter + this.hexCornerVectors[1];
      Vector2 rightBottomCorner = hexCenter + this.hexCornerVectors[2];
      Vector2 bottomCorner = hexCenter + this.hexCornerVectors[3];
      Vector2 leftBottomCorner = hexCenter + this.hexCornerVectors[4];
      Vector2 leftTopCorner = hexCenter + this.hexCornerVectors[5];

      polygon.Add(new Vertex(topCorner.x, topCorner.y));
      polygon.Add(new Vertex(rightTopCorner.x, rightTopCorner.y));
      polygon.Add(new Vertex(rightBottomCorner.x, rightBottomCorner.y));
      polygon.Add(new Vertex(bottomCorner.x, bottomCorner.y));
      polygon.Add(new Vertex(leftBottomCorner.x, leftBottomCorner.y));
      polygon.Add(new Vertex(leftTopCorner.x, leftTopCorner.y));
    }
    Debug.Log(polygon.Count);

    TriangleNet.Meshing.ConstraintOptions options =
    new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };
    testMesh = (TriangleNet.Mesh)polygon.Triangulate(options);
  }

  public void OnDrawGizmos() {

    Gizmos.color = Color.black;
    foreach (Vertex underWaterVertex in this.underWaterContour) {
      Gizmos.DrawSphere(new Vector3((float)underWaterVertex.x, (float)underWaterVertex.y, -1), 0.01f);
    }
    Gizmos.color = Color.white;
    foreach (Vertex shoreVerex in this.shoreContour) {
      Gizmos.DrawSphere(new Vector3((float)shoreVerex.x, (float)shoreVerex.y, -1), 0.01f);
    }

    if (testMesh == null) {
      // We're probably in the editor
      return;
    }

    /*Gizmos.color = Color.red;
    foreach (Edge edge in testMesh.Edges) {
      Vertex v0 = testMesh.vertices[edge.P0];
      Vertex v1 = testMesh.vertices[edge.P1];
      Vector3 p0 = new Vector3((float)v0.x, (float)v0.y);
      Vector3 p1 = new Vector3((float)v1.x, (float)v1.y);
      Gizmos.DrawLine(p0, p1);
    }*/
  }

  public void smoothShores(List<Hex> hexes) {
    foreach (Hex hex in hexes) {
      if (!hasTopLeftNeighbour(hex)) {
        if (hasLeftNeighbour(hex)) {
          smoothHorizontal(hex);
        } else {
          smoothLeftTopDiagonal(hex);
        }
      }
      if (!hasTopRightNeighbour(hex) && !hasRightNeighbour(hex)) {
        smoothRightTopDiagonal(hex);
      }
    }
  }

  public float getTerrainCurvingHeight(Vector2 vertex) {
    int left = Mathf.FloorToInt(vertex.x);
    int right = Mathf.CeilToInt(vertex.x);
    int bottom = Mathf.FloorToInt(vertex.y);
    int top = Mathf.CeilToInt(vertex.y);

    float f00 = getSurfaceCurving(left, bottom);
    float f10 = getSurfaceCurving(right, bottom);
    float f01 = getSurfaceCurving(left, top);
    float f11 = getSurfaceCurving(right, top);

    float x = vertex.x - left;
    float y = vertex.y - bottom;

    return f00 * (1 - x) * (1 - y) + f10 * x * (1 - y) + f01 * (1 - x) * y + f11 * x * y;
  }

  public float getSurfaceCurving(int x, int y) {
    try {
      float surfaceCurving = this.surfaceCurving[x - Mathf.FloorToInt(this.left), y - Mathf.FloorToInt(this.bottom)];
    } catch (System.Exception e) {
      Debug.Log(x - Mathf.FloorToInt(this.left));
      Debug.Log(y - Mathf.FloorToInt(this.bottom));
      return 0f;
    }
    return this.surfaceCurving[x - Mathf.FloorToInt(this.left), y - Mathf.FloorToInt(this.bottom)];
  }

  public void smoothHorizontal(Hex hex) {
    Vector2 leftTopCornerCoords = getLeftTopCornerDisplacementCoordinates(hex);
    hexCornersDisplacements[(int)leftTopCornerCoords.x, (int)leftTopCornerCoords.y] = randomCornerDisplacement();
  }

  public void smoothLeftTopDiagonal(Hex hex) {
    Vector2 leftTopCornerCoords = getLeftTopCornerDisplacementCoordinates(hex);
    hexCornersDisplacements[(int)leftTopCornerCoords.x, (int)leftTopCornerCoords.y] = centerOrientedRandomDisplacement(2);
  }

  public void smoothRightTopDiagonal(Hex hex) {
    Vector2 rightTopCornerCoords = getRightTopCornerDisplacementCoordinates(hex);
    hexCornersDisplacements[(int)rightTopCornerCoords.x, (int)rightTopCornerCoords.y] = centerOrientedRandomDisplacement(4);
  }

  public Vector2 centerOrientedRandomDisplacement(int hexCornerIndex) {
    return HexMathHelper.getHexCorners(this.getHexSize())[hexCornerIndex].normalized * Random.RandomRange(0.05f, 0.2f);
  }

  public Vector2 randomCornerDisplacement() {
    return new Vector2(0, Random.RandomRange(0.1f, 0.3f));
  }

  public Vector2 getLeftTopCornerDisplacementCoordinates(Hex hex) {
    Vector2 fixedCoordinates = getFixedCoordinates(hex);

    return new Vector2((int)fixedCoordinates.x * 2 + 1, (int)fixedCoordinates.y + 1);
  }

  public Vector2 getLeftTopCornerDisplacement(Hex hex) {
    Vector2 leftTopCornerCoords = getLeftTopCornerDisplacementCoordinates(hex);

    return hexCornersDisplacements[(int)leftTopCornerCoords.x, (int)leftTopCornerCoords.y];
  }

  public Vector2 getTopCornerDisplacementCoordinates(Hex hex) {
    Vector2 fixedCoordinates = getFixedCoordinates(hex);

    return new Vector2((int)fixedCoordinates.x * 2 + 2, (int)fixedCoordinates.y + 1);
  }

  public Vector2 getTopCornerDisplacement(Hex hex) {
    Vector2 topCornerCoords = getTopCornerDisplacementCoordinates(hex);

    return hexCornersDisplacements[(int)topCornerCoords.x, (int)topCornerCoords.y];
  }

  public Vector2 getRightTopCornerDisplacementCoordinates(Hex hex) {
    Vector2 fixedCoordinates = getFixedCoordinates(hex);

    return new Vector2((int)fixedCoordinates.x * 2 + 3, (int)fixedCoordinates.y + 1);
  }

  public Vector2 getRightTopCornerDisplacement(Hex hex) {
    Vector2 rightTopCornerCoords = getRightTopCornerDisplacementCoordinates(hex);

    return hexCornersDisplacements[(int)rightTopCornerCoords.x, (int)rightTopCornerCoords.y];
  }

  public Vector2 getLeftBottomCornerDisplacementCoordinates(Hex hex) {
    Vector2 fixedCoordinates = getFixedCoordinates(hex);

    return new Vector2((int)fixedCoordinates.x * 2, (int)fixedCoordinates.y);
  }

  public Vector2 getLeftBottomCornerDisplacement(Hex hex) {
    Vector2 leftBottomCornerCoords = getLeftBottomCornerDisplacementCoordinates(hex);

    return hexCornersDisplacements[(int)leftBottomCornerCoords.x, (int)leftBottomCornerCoords.y];
  }

  public Vector2 getBottomCornerDisplacementCoordinates(Hex hex) {
    Vector2 fixedCoordinates = getFixedCoordinates(hex);

    return new Vector2((int)fixedCoordinates.x * 2 + 1, (int)fixedCoordinates.y);
  }

  public Vector2 getBottomCornerDisplacement(Hex hex) {
    Vector2 bottomCornerCoords = getBottomCornerDisplacementCoordinates(hex);

    return hexCornersDisplacements[(int)bottomCornerCoords.x, (int)bottomCornerCoords.y];
  }

  public Vector2 getRightBottomCornerDisplacementCoordinates(Hex hex) {
    Vector2 fixedCoordinates = getFixedCoordinates(hex);

    return new Vector2((int)fixedCoordinates.x * 2 + 2, (int)fixedCoordinates.y);
  }

  public Vector2 getRightBottomCornerDisplacement(Hex hex) {
    Vector2 rightBottomCornerCoords = getRightBottomCornerDisplacementCoordinates(hex);

    return hexCornersDisplacements[(int)rightBottomCornerCoords.x, (int)rightBottomCornerCoords.y];
  }

  public Vector2 getFixedCoordinates(Hex hex) {
    return new Vector2(hex.x - minX, hex.y - minY);
  }

  public int getNeighbourRegionId(Hex hex, Vector2 neighbourDisplacement) {
    Vector2 fixedCoords = getFixedCoordinates(hex);
    int regionId = 0;
    try {
      regionId = regionIds[(int)fixedCoords.x + (int)neighbourDisplacement.x, (int)fixedCoords.y + (int)neighbourDisplacement.y];
    } catch (System.IndexOutOfRangeException e) {
      //return 0 as the hex is out of bounds
    }

    return regionId;
  }

  public bool hasTopLeftNeighbour(Hex hex) {
    return getNeighbourRegionId(hex, new Vector2(0, 1)) != 0;
  }

  private int[] simplestTriangles(Vector3[] vertices) {
    int[] triangles = new int[vertices.Length];
    for (int i = 0; i < triangles.Length; i++) {
      triangles[i] = i;
    }

    return triangles;
  }

  public bool hasLeftNeighbour(Hex hex) {
    return getNeighbourRegionId(hex, new Vector2(-1, 0)) != 0;
  }

  public bool hasTopRightNeighbour(Hex hex) {
    return getNeighbourRegionId(hex, new Vector2(1, 1)) != 0;
  }

  public bool hasRightNeighbour(Hex hex) {
    return getNeighbourRegionId(hex, new Vector2(1, 0)) != 0;
  }

  public void randomizeHexBounds(List<Vector3> vertices) {
    List<Vector3> randomizedVertices = new List<Vector3>();
    for (int i = 0; i < vertices.Count; i++) {
      //avoid centers
      if (i % 3 != 0) {
        Vector3 displacement = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);

        for (int duplicateIndex = 0; duplicateIndex < vertices.Count; duplicateIndex++) {
          if ((vertices[duplicateIndex] - vertices[i]).magnitude < 0.1f && duplicateIndex != i) {
            vertices[duplicateIndex] += displacement;
          }
        }
        vertices[i] += displacement;
      }
    }
  }

  void drawHex(Hex hex) {
    triangulateHex(hex);
    if (hex.regionId != 0) {
      this.colours.Add(this.getRegionById(hex.regionId).color);
      this.colours.Add(this.getRegionById(hex.regionId).color);
      this.colours.Add(this.getRegionById(hex.regionId).color);
      this.colours.Add(this.getRegionById(hex.regionId).color);
      this.colours.Add(this.getRegionById(hex.regionId).color);
      this.colours.Add(this.getRegionById(hex.regionId).color);
      this.colours.Add(this.getRegionById(hex.regionId).color);
      this.colours.Add(this.getRegionById(hex.regionId).color);
      this.colours.Add(this.getRegionById(hex.regionId).color);
      this.colours.Add(this.getRegionById(hex.regionId).color);
      this.colours.Add(this.getRegionById(hex.regionId).color);
      this.colours.Add(this.getRegionById(hex.regionId).color);
      this.colours.Add(this.getRegionById(hex.regionId).color);
      this.colours.Add(this.getRegionById(hex.regionId).color);
      this.colours.Add(this.getRegionById(hex.regionId).color);
      this.colours.Add(this.getRegionById(hex.regionId).color);
      this.colours.Add(this.getRegionById(hex.regionId).color);
      this.colours.Add(this.getRegionById(hex.regionId).color);
    }
    //hexObject.GetComponent<SpriteRenderer>().color = getColorByTerrainType(hex.terrainType);
  }

  void triangulateHex(Hex hex) {
    Vector2 hexCenter = HexMathHelper.hexToWorldCoords(new Vector2(hex.x, hex.y), this.getHexSize());

    Vector2 topCorner = hexCenter + this.hexCornerVectors[0] + getTopCornerDisplacement(hex);
    Vector2 rightTopCorner = hexCenter + this.hexCornerVectors[1] + getRightTopCornerDisplacement(hex);
    Vector2 rightBottomCorner = hexCenter + this.hexCornerVectors[2] + getRightBottomCornerDisplacement(hex);
    Vector2 bottomCorner = hexCenter + this.hexCornerVectors[3] + getBottomCornerDisplacement(hex);
    Vector2 leftBottomCorner = hexCenter + this.hexCornerVectors[4] + getLeftBottomCornerDisplacement(hex);
    Vector2 leftTopCorner = hexCenter + this.hexCornerVectors[5] + getLeftTopCornerDisplacement(hex);

    this.addTriangle(
      hexCenter,
      topCorner,
      rightTopCorner
      );
    //this.uv.Add(new Vector2(0.5f, 0.5f));
    //this.uv.Add(new Vector2(0.5f, 1f));
    //this.uv.Add(new Vector2(1f, 0.75f));
    this.addTriangle(
      hexCenter,
      rightTopCorner,
      rightBottomCorner
      );
    //this.uv.Add(new Vector2(0.5f, 0.5f));
    //this.uv.Add(new Vector2(1f, 0.75f));
    //this.uv.Add(new Vector2(1f, 0.25f));
    this.addTriangle(
      hexCenter,
      rightBottomCorner,
      bottomCorner
      );
    //this.uv.Add(new Vector2(0.5f, 0.5f));
    //this.uv.Add(new Vector2(1f, 0.25f));
    ///this.uv.Add(new Vector2(0.5f, 0f));
    this.addTriangle(
      hexCenter,
      bottomCorner,
      leftBottomCorner
      );
    //this.uv.Add(new Vector2(0.5f, 0.5f));
    //this.uv.Add(new Vector2(0.5f, 0f));
    //this.uv.Add(new Vector2(0f, 0.25f));
    this.addTriangle(
      hexCenter,
      leftBottomCorner,
      leftTopCorner
      );
    //this.uv.Add(new Vector2(0.5f, 0.5f));
    //this.uv.Add(new Vector2(0f, 0.25f));
    //this.uv.Add(new Vector2(0f, 0.75f));
    this.addTriangle(
      hexCenter,
      leftTopCorner,
      topCorner
      );
    //this.uv.Add(new Vector2(0.5f, 0.5f));
    //this.uv.Add(new Vector2(0f, 0.75f));
    //this.uv.Add(new Vector2(0.5f, 1f));
    this.uv.AddRange(AtlasTextureHelper.getUVForTexture(1, 1));
  }

  void addTriangle(Vector2 verticeOne, Vector2 verticeTwo, Vector2 verticeThree) {
    int vertexIndex = this.vertices.Count;
    vertices.Add(verticeOne);
    vertices.Add(verticeTwo);
    vertices.Add(verticeThree);
    triangles.Add(vertexIndex);
    triangles.Add(vertexIndex + 1);
    triangles.Add(vertexIndex + 2);
  }

  public void paintHexesByTerrain() {
    this.colours = new List<Color32>();
    foreach (Hex hex in map.hexes) {
      Color32 newHexColour = getColorByTerrainType(hex.terrainType);
      addColourForEntireHex(newHexColour);
    }
    this.mesh.colors32 = this.colours.ToArray();
  }

  public void addColourForEntireHex(Color32 color) {
    for (int i = 0; i < 18; i++) {
      this.colours.Add(color);
    }
  }

  public void paintHexesByClimate() {
    this.colours = new List<Color32>();
    foreach (Hex hex in map.hexes) {
      addColourForEntireHex(getColorByClimateType(hex.climateType));
    }
    this.mesh.colors32 = this.colours.ToArray();
  }

  public void paintHexesByWater() {
    this.colours = new List<Color32>();
    foreach (Hex hex in map.hexes) {
      //GameObject hexObject = this.findHexObject(new Vector2(hex.x, hex.y));
      //if (hexObject != null) {
      // hexObject.GetComponent<SpriteRenderer>().color = getColorByHexWater(hex);
      // }
      addColourForEntireHex(getColorByHexWater(hex));
    }
    this.mesh.colors32 = this.colours.ToArray();
  }

  public void paintHexesByRegions() {
    this.colours = new List<Color32>();
    foreach (Hex hex in map.hexes) {
      addColourForEntireHex(this.getRegionById(hex.regionId).color);
    }
    this.mesh.colors32 = this.colours.ToArray();
  }

  public Color32 getColorByTerrainType(TerrainType terrainType) {
    switch (terrainType) {
      case TerrainType.HIGH: return new Color32(255, 20, 20, 255);
      case TerrainType.ELEVATED: return new Color32(255, 110, 110, 255);
      case TerrainType.NORMAL: return new Color32(255, 200, 200, 255);
      case TerrainType.LOW:
      default: return new Color32(255, 255, 255, 255);
    }
  }

  public Color32 getColorByClimateType(ClimateType climateType) {
    switch (climateType) {
      case ClimateType.COOL: return Color.blue;
      case ClimateType.WARM: return Color.red;
      case ClimateType.TEMPERATE:
      default: return Color.green;
    }
  }

  public Color32 getHexColor(Hex hex) {
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

  public Color32 getColorByHexWater(Hex hex) {
    if (hex.hasTag(CriticalResource.WATER.name)) {
      return Color.blue;
    }

    if (hex.hasTag(CriticalResource.WATER.adjacentHexName)) {
      return new Color32(200, 200, 255, 255);
    }


    return Color.white;
  }

  public float getHexSize() {
    return 1;
  }

  public static MeshMapController getInstance() {
    return GameObject.FindGameObjectWithTag("Map").GetComponent<MeshMapController>();
  }
}
