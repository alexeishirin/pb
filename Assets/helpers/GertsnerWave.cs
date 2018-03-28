using UnityEngine;

public class GertsnerWave {
  public float A;
  public float Q;// 1/wA
  public Vector2 D;
  public float L;//wavelength
  public float w;// 2/L frequency
  public float S;// speed
  public float fi;

  public GertsnerWave(float amplitude, float waveLength, float speed, Vector2 direction) {
    this.A = amplitude;
    this.L = waveLength;
    this.w = 2 * Mathf.PI / L;
    this.S = speed;
    this.Q = 1 / (w * A);
    this.D = direction;
    this.fi = 2 * Mathf.PI * S / L;
  }

  public Vector3 getPointShift(Vector2 point, float t) {
    float x = Q * A * D.x * Mathf.Cos(w * Vector2.Dot(D, point) + fi * t);
    float y = Q * A * D.y * Mathf.Cos(w * Vector2.Dot(D, point) + fi * t);
    float z = A * Mathf.Sin(w * Vector2.Dot(D, point) + fi * t);

    return new Vector3(x, y, -z);
  }
}