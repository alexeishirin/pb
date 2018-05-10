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

  public GameObject ocean;
  public GameObject rivers;
  public GameObject lakes;

  public GameObject treePrefab;

  public System.Random randomGenerator;

  public int specifiedMapSeed = 0;

  public Map map;

  private UnityEngine.Mesh mesh;

  private float polygonDensity = 0.2f * 1f;

  private float riverDensity = 0.15f;

  void Start() {
  }

  void Update() {
    
  }

  public void regenerateMap(Map newMap) {
    clearMesh();
    this.map = newMap;
    drawMap();
  }

  public void clearMesh() {
  /*  this.mesh.vertices = vertices.ToArray();
    this.mesh.triangles = triangles.ToArray();
    this.mesh.colors32 = colours.ToArray();
    this.mesh.uv = new List<Vector2>().ToArray();*/
  }

  public void clearMap() {
    this.map = null;
  }

  public void setMapSeed(int newSeed) {
    this.specifiedMapSeed = newSeed;
  }

  public void drawMap() {
    float heightSeed = Random.Range(0, 100);

    MeshGenerator meshGenerator = new MeshGenerator(this.getHexSize(), map, polygonDensity, 
      riverDensity, heightSeed, elevatedHeightmap, mountainHeightmap, rivers);
    MapMesh mapMesh = meshGenerator.generateMapMesh();
    this.GetComponent<MeshFilter>().mesh = mesh = mapMesh.landMesh;
    this.GetComponent<MeshCollider>().sharedMesh = this.GetComponent<MeshFilter>().mesh;
    this.lakes.GetComponent<MeshFilter>().mesh = mapMesh.innerWaterMesh;
    this.ocean.GetComponent<MeshFilter>().mesh = mapMesh.oceanMesh;

    generateTrees(heightSeed);
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

  public MapVertex convertToMapVertex(Vertex vertex) {
    Hex vertexHex = map.getHex(HexMathHelper.worldToHexCoords(new Vector2((float)vertex.x, (float)vertex.y), this.getHexSize()));

    return new MapVertex(vertex.x, vertex.y, vertexHex);
  }

  public float getHexSize() {
    return 1;
  }

  public static MeshMapController getInstance() {
    return GameObject.FindGameObjectWithTag("Map").GetComponent<MeshMapController>();
  }

}
