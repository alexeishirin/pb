using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIController : MonoBehaviour, IPointerClickHandler {

  public GameObject settingsPanel;
  public GameObject hexInfoPanel;
  public GameObject startingScreenPanel;
  public GameObject startingMenuPanel;
  public GameObject inGamePanel;
  public GameObject partyPanel;
  public GameObject timePanel;

  public HexHighlightController hexHighlightController;
  public Camera mainCamera;
  public Camera characterCamera;

  public Vector2 highightedHex;

  public bool isClickable = false;

  Plane mapPlane = new Plane(Vector3.back, Vector3.zero);

	// Use this for initialization
	void Start () {
    this.mainCamera.enabled = true;
    this.characterCamera.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
    this.showHexInfo();
    this.highlightTile();
    this.switchCameras();
  }

  public void switchCameras() {
    if(Input.GetKeyUp(KeyCode.Tab)) {
      if(this.mainCamera.enabled) {
        this.mainCamera.enabled = false;
        this.characterCamera.enabled = true;     
      } else {
        this.mainCamera.enabled = true;
        this.characterCamera.enabled = false;
      }
    }
  }

  public void updateTimePanel(int newTime, int dayLength, int nightLength)
  {
    this.timePanel.GetComponent<TimePanelController>().updateTime(newTime, dayLength, nightLength);
  }

  public void highlightTile()
  {
    Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
    float enter = 0.0f;
    if(mapPlane.Raycast(ray, out enter)) {
      mouseWorldPosition = ray.GetPoint(enter);
    }

    float hexSize = MeshMapController.getInstance().getHexSize();
    Vector2 newlyHighlightedHex = HexMathHelper.worldToHexCoords(mouseWorldPosition, hexSize);
    if(newlyHighlightedHex != this.highightedHex && HexMathHelper.hexDistance(newlyHighlightedHex, PartyController.getInstance().partyPosition) < 10)
    {
      this.highightedHex = newlyHighlightedHex;
      hexHighlightController.highlightHex(this.highightedHex);
      
    } else if(newlyHighlightedHex != this.highightedHex)
    {
      hexHighlightController.hideHighlight();
      this.highightedHex = new Vector2(0, 0);
    }

  }

  public void setCameraPosition(Vector2 newCameraPosition) {
    this.mainCamera.transform.position = new Vector3(newCameraPosition.x, newCameraPosition.y, this.mainCamera.transform.position.z);
  }

  public void toggleSettingsPanel()
  {
    if(settingsPanel.active)
    {
      settingsPanel.SetActive(false);
    } else
    {
      settingsPanel.SetActive(true);
    }
  }

  public void togglePartyPanel()
  {
    if (this.partyPanel.active)
    {
      this.partyPanel.SetActive(false);
    }
    else
    {
      this.partyPanel.SetActive(true);
    }
  }

  public void showPartyInfo()
  {
    this.partyPanel.GetComponent<PartyPanelController>().showPartyInfo();
  }

  public void updateSettingsPanel(MapGenerationSettings mapGenerationSettings, int mapSeed)
  {
    SettingsPanelController settingsPanelController = settingsPanel.GetComponent<SettingsPanelController>();
    settingsPanelController.updatePanel(mapGenerationSettings, mapSeed);
  }

  public void showHexInfo(Hex hex)
  {
    HexInfoPanelController hexInfoPanelController = hexInfoPanel.GetComponent<HexInfoPanelController>();
    hexInfoPanelController.showHexInfo(hex);
  }

  public void showHexInfo() {
    Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
    float enter = 0.0f;
    if (mapPlane.Raycast(ray, out enter)) {
      mouseWorldPosition = ray.GetPoint(enter);
    }
    float hexSize = MeshMapController.getInstance().getHexSize();
    Vector2 newlyHighlightedHexCoords = HexMathHelper.worldToHexCoords(mouseWorldPosition, hexSize);
    if (MeshMapController.getInstance() != null && MeshMapController.getInstance().map != null && MeshMapController.getInstance().map.getHex(newlyHighlightedHexCoords) != null) {
      Hex hex = MeshMapController.getInstance().map.getHex(newlyHighlightedHexCoords);
      HexInfoPanelController hexInfoPanelController = hexInfoPanel.GetComponent<HexInfoPanelController>();
      hexInfoPanelController.showHexInfo(hex);
    }
  }

  public void toggleHexInfoPanel()
  {
    this.hexInfoPanel.SetActive(!this.hexInfoPanel.active);
  }


  public static UIController getInstance()
  {
    return GameObject.FindGameObjectWithTag("UIController").GetComponent<UIController>();
  }

  public IEnumerator gameStartAnimation()
  {
    Text startingScreenText = this.startingScreenPanel.GetComponent<StartingScreenPanelController>().mainText;
    Text startingScreenSecondaryText = this.startingScreenPanel.GetComponent<StartingScreenPanelController>().secondaryText;
    Image startingScreenImage = this.startingScreenPanel.GetComponent<StartingScreenPanelController>().mainImage;
    yield return fadeTextInAndOut(startingScreenText);
    startingScreenText.text = "In Association With";
    yield return fadeTextInAndOut(startingScreenText);
    startingScreenText.text = "Epic Splash Screen And Menu Makers";
    yield return fadeTextInAndOut(startingScreenText);
    startingScreenText.text = "Present";
    yield return fadeTextInAndOut(startingScreenText);
    yield return new WaitForSeconds(1f);
    startingScreenImage.color = new Color32(255, 255, 255, 255);
    startingScreenText.text = "PLANET BREAK";
    startingScreenText.color = new Color32(200, 150, 200, 255);
    yield return new WaitForSeconds(3f);
    yield return fadeTextIn(startingScreenSecondaryText);
    this.isClickable = true;
  }

  public IEnumerator fadeTextInAndOut(Text text)
  {
    float currentLerpTime = 0;
    float lerpTime = 1.2f;
    Color32 textColor = text.color;
    while (currentLerpTime < lerpTime)
    {
      float t = currentLerpTime / lerpTime;
      t = t * t * t * (t * (6f * t - 15f) + 10f);
      text.color = Color32.Lerp(new Color32(textColor.r, textColor.g, textColor.b, 0), new Color32(textColor.r, textColor.g, textColor.b, 255), t);
      currentLerpTime += 0.01f;
      yield return new WaitForSeconds(0.01f);
    }
    yield return new WaitForSeconds(1f);
    currentLerpTime = 0;
    while (currentLerpTime < lerpTime)
    {
      float t = currentLerpTime / lerpTime;
      t = t * t * t * (t * (6f * t - 15f) + 10f);
      text.color = Color32.Lerp(new Color32(textColor.r, textColor.g, textColor.b, 255), new Color32(textColor.r, textColor.g, textColor.b, 0), t);
      currentLerpTime += 0.01f;
      yield return new WaitForSeconds(0.01f);
    }
  }

  public IEnumerator fadeTextIn(Text text)
  {
    float currentLerpTime = 0;
    float lerpTime = 1.2f;
    Color32 textColor = text.color;
    while (currentLerpTime < lerpTime)
    {
      float t = currentLerpTime / lerpTime;
      t = t * t * t * (t * (6f * t - 15f) + 10f);
      text.color = Color32.Lerp(new Color32(textColor.r, textColor.g, textColor.b, 0), new Color32(textColor.r, textColor.g, textColor.b, 255), t);
      currentLerpTime += 0.01f;
      yield return new WaitForSeconds(0.01f);
    }
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    if(this.isClickable)
    {
      this.startingScreenPanel.gameObject.SetActive(false);
      this.startingMenuPanel.gameObject.SetActive(true);
      this.isClickable = false;
    }
  }

  public void highlightMenuItem(RectTransform menuItem)
  {
    this.startingMenuPanel.GetComponent<StartingMenuPanelController>().highlightMenuItem(menuItem);
  }
}
