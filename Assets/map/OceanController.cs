using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class OceanController : MonoBehaviour {
  private float currentPerlinNoise;
  private const float LERP_TIME = 0.7f;
  private float currentLerpingTime = LERP_TIME;
  private float[] velocities = null;
  private float[] steerings = null;

  public float mass = 1;
  public float maxSpeed = 0.03f;
  public float maxForce = 0.01f;

  void Update() {
      animateWater();
  }

  public void animateWater() {
    Mesh mesh = this.GetComponent<MeshFilter>().mesh;
    if (mesh == null) {
      return;
    }

    Vector3[] vertices = mesh.vertices;
    if (velocities == null || velocities.Length != vertices.Length) {
      velocities = new float[vertices.Length];
    }

    if (steerings == null || steerings.Length != vertices.Length) {
      steerings = new float[vertices.Length];
    }
    this.currentLerpingTime += Time.deltaTime;
    if (currentLerpingTime >= LERP_TIME) {
      currentLerpingTime = 0f;
      this.currentPerlinNoise = Random.Range(0, 100);
    }/* else {
      float previousTime = currentLerpingTime - Time.deltaTime;
      float t = Time.deltaTime / (LERP_TIME - previousTime);

      for (int i = 0; i < vertices.Length; i++) {
        vertices[i].z = Mathf.Lerp(vertices[i].z, TerrainHelper.getTerrainCurvingHeight(vertices[i], currentPerlinNoise), t);
      }

      mesh.vertices = vertices;
      mesh.RecalculateNormals();
    }*/
    for (int i = 0; i < vertices.Length; i++) {
      steerings[i] = getWanderForce(vertices[i]);
      velocities[i] = Mathf.Clamp(velocities[i] + steerings[i], -maxSpeed, +maxSpeed);
      vertices[i].z += velocities[i] * Time.deltaTime;
    }

    mesh.vertices = vertices;
    mesh.RecalculateNormals();
  }

  private float getWanderForce(Vector3 vertice) {
    if (vertice.z > 0.2f) {
      return -maxForce;
    } else if (vertice.z < 0.1f) {
      return maxForce;
    } else {
      return getRandomWanderForce(vertice);
    }
  }

  private float getRandomWanderForce(Vector3 vertice) {
    //return Random.Range(-0.05f, 0.05f);
    return TerrainHelper.getWaterCurvingHeight(vertice, currentPerlinNoise);
  }
}