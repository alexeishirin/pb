using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
  public int scrollDistance = 10;
  public float maxScrollSpeed = 1f;
  public float scrollSpeedAdjust = 0.1f;
  public float zoomTime = 1.5f;
  public float zoomStep = 1f;

  private float minX = 0.0f;
  private float maxX;
  private float minY;
  private float maxY;

  private float xScrollSpeed = 0;
  private float yScrollSpeed = 0;
  private float epsilon = 0.1f;
  private float currentZoomTime = 1.5f;

  private float startingZoomZ = -4f;
  private float targetZoomZ = -4f;
  private float zoomSpeed = 0f;

  private const float maxZoomZ = -2f;
  private const float minZoomZ = -6f;

  private const float maxZoomAngle = -45;
  private const float minZoomAngle = -20;

  // Use this for initialization
  void Start () {
    Camera camera = this.GetComponent<Camera>();
    camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, startingZoomZ);
    camera.transform.eulerAngles = new Vector3(angleFromZoom(camera.transform.position.z), 0, 0);
  }

  private void Update()
  {
    Camera camera = this.GetComponent<Camera>();
    Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(Input.mousePosition);
  }

  void FixedUpdate()
  {

    scrollX();
    scrollY();
    zoom();
  }

  void zoom()
  {
    
    float mouseWheelInput = Input.GetAxis("Mouse ScrollWheel");
    if(mouseWheelInput < 0)
    {
      this.setNewZoom(targetZoomZ - zoomStep, Input.mousePosition);
    } else if (mouseWheelInput > 0)
    {
      this.setNewZoom(targetZoomZ + zoomStep, Input.mousePosition);
    }
    if(currentZoomTime < zoomTime)
    {
      currentZoomTime += Time.deltaTime;
    }
    if (currentZoomTime > zoomTime)
    {
      currentZoomTime = zoomTime;
    }
    if (currentZoomTime != zoomTime)
    {
      float t = currentZoomTime / zoomTime;
      t = t * t * t * (t * (6f * t - 15f) + 10f);
      Camera camera = this.GetComponent<Camera>();
      float oldZ = camera.transform.position.z;
      float newZ = Mathf.Lerp(startingZoomZ, targetZoomZ, t);
      Vector3 localUpVector = camera.transform.TransformDirection(Vector3.up).normalized;
      float deltaZ = oldZ - newZ;
      float deltaY = deltaZ / localUpVector.z * localUpVector.y;
      camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y + deltaY * 0.01f, newZ);
      camera.transform.eulerAngles = new Vector3(angleFromZoom(camera.transform.position.z), 0, 0);
    }
  }

  void scrollX()
  {
    if(this.isTooCloseToTheEdgeOfMapX())
    {
      xScrollSpeed = Mathf.MoveTowards(xScrollSpeed, 0, Time.deltaTime * scrollSpeedAdjust * 2.0f);
      if (Mathf.Abs(xScrollSpeed) < epsilon)
      {
        xScrollSpeed = 0;
      }

      return;
    }

    float mousePosX = Input.mousePosition.x;
    if (mousePosX <= scrollDistance && mousePosX >= 0)
    {
      xScrollSpeed = Mathf.MoveTowards(xScrollSpeed, -maxScrollSpeed, Time.deltaTime * scrollSpeedAdjust);
    }
    else if (mousePosX >= Screen.width - scrollDistance && mousePosX <= Screen.width)
    {
      xScrollSpeed = Mathf.MoveTowards(xScrollSpeed, maxScrollSpeed, Time.deltaTime * scrollSpeedAdjust);
    }
    else
    {
      xScrollSpeed = Mathf.MoveTowards(xScrollSpeed, 0, Time.deltaTime * scrollSpeedAdjust * 1.0f);
      if (Mathf.Abs(xScrollSpeed) < epsilon)
      {
        xScrollSpeed = 0;
      }
    }

    transform.Translate(Vector3.right * xScrollSpeed * Time.deltaTime, Space.World);
    //transform.position = new Vector3(Mathf.Clamp(transform.position.x, minX, maxX), Mathf.Clamp(transform.position.y, minY, maxY), transform.position.z);
  }

  void scrollY()
  {
    if (this.isTooCloseToTheEdgeOfMapY())
    {
      yScrollSpeed = Mathf.MoveTowards(yScrollSpeed, 0, Time.deltaTime * scrollSpeedAdjust * 2.0f);
      if (Mathf.Abs(yScrollSpeed) < epsilon)
      {
        yScrollSpeed = 0;
      }

      return;
    }
    float mousePosY = Input.mousePosition.y;
    if (mousePosY <= scrollDistance && mousePosY > 0)
    {
      yScrollSpeed = Mathf.MoveTowards(yScrollSpeed, -maxScrollSpeed, Time.deltaTime * scrollSpeedAdjust);
    }
    else if (mousePosY >= Screen.height - scrollDistance && mousePosY <= Screen.height)
    {
      yScrollSpeed = Mathf.MoveTowards(yScrollSpeed, maxScrollSpeed, Time.deltaTime * scrollSpeedAdjust);
    }
    else
    {
      yScrollSpeed = Mathf.MoveTowards(yScrollSpeed, 0, Time.deltaTime * scrollSpeedAdjust * 1.0f);
      if (Mathf.Abs(yScrollSpeed) < epsilon)
      {
        yScrollSpeed = 0;
      }
    }

    transform.Translate(Vector3.up * yScrollSpeed * Time.deltaTime, Space.World);
    //transform.position = new Vector3(Mathf.Clamp(transform.position.x, minX, maxX), Mathf.Clamp(transform.position.y, minY, maxY), transform.position.z);
  }

  public void setNewZoom(float newZoom, Vector3 zoomCenter)
  {
    newZoom = Mathf.Max(newZoom, minZoomZ);
    newZoom = Mathf.Min(newZoom, maxZoomZ);
    Debug.Log(newZoom);
    this.targetZoomZ = newZoom;
    Camera camera = this.GetComponent<Camera>();
    this.startingZoomZ = camera.transform.position.z;
    if (startingZoomZ != targetZoomZ)
    {
      this.currentZoomTime = 0;
    }
  }

  public void setMapBoundaries(float newOrthographicSize)
  {
    float halfScreenHeight = newOrthographicSize;
    float halfScreenWidth = halfScreenHeight * Screen.width / Screen.height;
    minX = halfScreenWidth - 1000;
    int mapLength = 100;
    maxX = mapLength - halfScreenWidth + 100;

    minY = -500f + halfScreenHeight;
    maxY = 500f - halfScreenHeight;
    transform.position = new Vector3(Mathf.Clamp(transform.position.x, minX, maxX), Mathf.Clamp(transform.position.y, minY, maxY), transform.position.z);
  }

  public bool isTooCloseToTheEdgeOfMapX()
  {
    return false;
    float maxDeceleration = scrollSpeedAdjust * 2.0f;
    float distanceToTheEdge = this.xScrollSpeed > 0 ? this.maxX - transform.position.x : transform.position.x - this.minX;

    return this.xScrollSpeed != 0 && distanceToTheEdge * maxDeceleration <= this.xScrollSpeed * this.xScrollSpeed;
  }

  public float angleFromZoom(float zoom) {
    return Mathf.Lerp(minZoomAngle, maxZoomAngle, Mathf.InverseLerp(minZoomZ, maxZoomZ, zoom));
  }

  public bool isTooCloseToTheEdgeOfMapY()
  {
    return false;
    float maxDeceleration = scrollSpeedAdjust * 2.0f;
    float distanceToTheEdge = this.yScrollSpeed > 0 ? this.maxY - transform.position.y : transform.position.y - this.minY;

    return this.yScrollSpeed != 0 && distanceToTheEdge * maxDeceleration <= this.yScrollSpeed * this.yScrollSpeed;
  }
}
