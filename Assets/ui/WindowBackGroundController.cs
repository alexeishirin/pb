using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WindowBackGroundController : MonoBehaviour {

  public int xSize, ySize, xLengthRowDispersion;

  private List<Vector3> vertices;
  private List<int> triangles;

  private Vector3[] velocities;
  private Vector3[] steerings;
  private Vector3[] centers;

  public float mass = 15;
  public float maxSpeed = 3;
  public float maxForce = 15;
  public float circleRadius = 1f;
  public float maxRadius = 0.3f;

  public float scale = 1f;

  private Mesh mesh;

  public float lerpColoursTime = 2f;
  private float currentColourLerpTime;
  private Color[] startingColours;
  private Color[] targetColours;

  public void Awake() {
    //StartCoroutine(Generate());
    Generate();
    initVelocities();

    currentColourLerpTime = lerpColoursTime;
  }

  public void Update() {
    for (int i = 0; i < vertices.Count; i++) {
      steerings[i] = getWanderForce(i);
      steerings[i] = steerings[i] / mass;
      velocities[i] = Vector2.ClampMagnitude(velocities[i] + steerings[i], maxSpeed);
      vertices[i] += velocities[i] * Time.deltaTime;
    }
    mesh.vertices = this.duplicateVertices(vertices, triangles).ToArray();

    lerpColours();
  }

  private void lerpColours() {
    currentColourLerpTime += Time.deltaTime;
    if (currentColourLerpTime >= lerpColoursTime) {
      currentColourLerpTime = 0f;
      this.startingColours = targetColours;
      targetColours = new Color[mesh.vertices.Length];
      for (int colorIndex = 0; colorIndex < targetColours.Length; colorIndex += 3) {
        int deviation = Random.Range(0, 50);
        targetColours[colorIndex] = targetColours[colorIndex + 1] = targetColours[colorIndex + 2]
          = new Color32((byte)(255 - deviation), (byte)(255 - deviation), (byte)(255 - deviation), 255);
      }
    }
    Color[] currentColours = new Color[startingColours.Length];
    float t = currentColourLerpTime / lerpColoursTime;
    for (int colorIndex = 0; colorIndex < currentColours.Length; colorIndex += 3) {
      currentColours[colorIndex] = currentColours[colorIndex + 1] = currentColours[colorIndex + 2] =
        Color.Lerp(startingColours[colorIndex], targetColours[colorIndex], t * t);
    }
    mesh.colors = currentColours;

  }

  private void initVelocities() {
    velocities = new Vector3[this.vertices.Count];
    steerings = new Vector3[this.vertices.Count];
    centers = new Vector3[this.vertices.Count];

    for (int i = 0; i < centers.Length; i++) {
      centers[i] = vertices[i];
      velocities[i] = Random.insideUnitCircle;
      steerings[i] = getWanderForce(i);
    }
  }

  private void Generate() {
    WaitForSeconds wait = new WaitForSeconds(0.2f);

    int[] xRowLengths = new int[ySize + 1];
    for (int index = 0; index <= ySize; index++) {
      xRowLengths[index] = Random.Range(xSize + 1 - xLengthRowDispersion, xSize + xLengthRowDispersion + 2);
    }

    int verticesLength = 0;
    foreach (int xRowLength in xRowLengths) {
      verticesLength += xRowLength;
    }

    vertices = new List<Vector3>();
    int y = 0;
    int i = 0;
    foreach (int xRowLength in xRowLengths) {
      float xStep = (float)(xSize) / (float)(xRowLength - 1);
      float yStep = 1f;
      for (int x = 0; x < xRowLength; x++) {
        if (x == 0 || x == xRowLength - 1 || y == 0 || y == xRowLengths.Length - 1) {
          float verticeX = Random.Range(xStep * ((float)x - 0.1f), xStep * ((float)x + 0.1f));
          float verticeY = Random.Range(yStep * ((float)y - 0.1f), yStep * ((float)y + 0.1f));
          vertices.Add(new Vector3(verticeX, verticeY));
        } else {
          float verticeX = Random.Range(xStep * ((float)x - 0.3f), xStep * ((float)x + 0.3f));
          float verticeY = Random.Range(yStep * ((float)y - 0.3f), yStep * ((float)y + 0.3f));
          vertices.Add(new Vector3(verticeX, verticeY));
        }

        i++;
      }
      y++;
    }

    triangles = new List<int>();

    int verticesOffset = 0;
    for (int rowIndex = 0; rowIndex < xRowLengths.Length - 1; rowIndex++) {
      int bottomRowIndex = 0;
      int upperRowIndex = 0;
      verticesOffset += rowIndex == 0 ? 0 : xRowLengths[rowIndex - 1];
      while (bottomRowIndex < (xRowLengths[rowIndex] - 1) || upperRowIndex < (xRowLengths[rowIndex + 1] - 1)) {
        //yield return wait;
        triangles.Add(bottomRowIndex + verticesOffset);
        triangles.Add(xRowLengths[rowIndex] + upperRowIndex + verticesOffset);

        if ((bottomRowIndex == xRowLengths[rowIndex] - 1)) {
          upperRowIndex++;
          triangles.Add(xRowLengths[rowIndex] + upperRowIndex + verticesOffset);
          continue;
        }
        if (upperRowIndex == xRowLengths[rowIndex + 1] - 1) {
          bottomRowIndex++;
          triangles.Add(bottomRowIndex + verticesOffset);
          continue;
        }
        if (vertices[bottomRowIndex + 1 + verticesOffset].x <= vertices[xRowLengths[rowIndex] + upperRowIndex + 1 + verticesOffset].x) {
          bottomRowIndex++;
          triangles.Add(bottomRowIndex + verticesOffset);
          continue;
        } else {
          upperRowIndex++;
          triangles.Add(xRowLengths[rowIndex] + upperRowIndex + verticesOffset);
          continue;
        }
      }
    }


    GetComponent<MeshFilter>().mesh = mesh = new Mesh();
    mesh.name = "Procedural Grid";
    mesh.vertices = this.duplicateVertices(vertices, triangles).ToArray();
    mesh.triangles = simplestTriangles(mesh.vertices);
    mesh.RecalculateNormals();
    Color[] colors = new Color[mesh.vertices.Length];
    for (int colorIndex = 0; colorIndex < colors.Length; colorIndex += 3) {
      int deviation = Random.Range(0, 50);
      colors[colorIndex] = colors[colorIndex + 1] = colors[colorIndex + 2]
        = new Color32((byte)(255 - deviation), (byte)(255 - deviation), (byte)(255 - deviation), 255);
    }
    this.startingColours = colors;
    this.targetColours = colors;
    mesh.colors = colors;
  }

  private int[] simplestTriangles(Vector3[] vertices) {
    int[] triangles = new int[vertices.Length];
    for (int i = 0; i < triangles.Length; i++) {
      triangles[i] = i;
    }

    return triangles;
  }

  private List<Vector3> duplicateVertices(List<Vector3> initialVertices, List<int> triangles) {
    List<Vector3> duplicatedVertices = new List<Vector3>();
    for (int triangleIndex = 0; triangleIndex < triangles.Count; triangleIndex++) {
      duplicatedVertices.Add(initialVertices[triangles[triangleIndex]]);
      //triangles[triangleIndex] = duplicatedVertices.Count - 1;
    }

    return duplicatedVertices;
  }

  private List<int> duplicateTriangles(List<int> initialTriangles) {
    int previousVerticeIndex = -1;
    int fixedVerticeOffset = 0;
    for (int verticeIndex = 0; verticeIndex < initialTriangles.Count; verticeIndex++) {
      if (verticeIndex == previousVerticeIndex) {
        fixedVerticeOffset++;
      }
      initialTriangles[verticeIndex] += fixedVerticeOffset;
      previousVerticeIndex = verticeIndex;
    }

    return initialTriangles;
  }

  private Vector2 getWanderForce(int verticeIndex) {
    if ((centers[verticeIndex] - vertices[verticeIndex]).magnitude > maxRadius) {
      return (centers[verticeIndex] - vertices[verticeIndex]).normalized * maxForce;
    } else {
      return getRandomWanderForce(verticeIndex);
    }
  }

  private Vector2 getRandomWanderForce(int verticeIndex) {
    Vector2 circleCenter = velocities[verticeIndex].normalized;
    Vector2 displacement = Random.insideUnitCircle.normalized * circleRadius;

    return Vector2.ClampMagnitude(circleCenter + displacement, maxForce);
  }

  private void OnDrawGizmos() {
    /*if (vertices == null) {
      return;
    }
    Gizmos.color = Color.black;
    for (int i = 0; i < vertices.Length; i++) {
      Gizmos.DrawSphere(vertices[i], 0.1f);
    }*/
  }


}
