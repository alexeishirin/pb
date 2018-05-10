using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour {
  public MapGenerationSettings mapGenerationSettings;
  public PartyGenerationSettings partyGenerationSettings;
  public MapData mapData;
  public System.Random randomGenerator;
  public string applicationPersistentPath;

  public int currentTime;

  // Use this for initialization
  void Start () {
    applicationPersistentPath = Application.persistentDataPath;
    //StartCoroutine(startGame());
	}

  public IEnumerator startGame()
  {
    yield return UIController.getInstance().gameStartAnimation();
  }

  public void quit()
  {
    Application.Quit();
  }

  public void generateWorld()
  {
    Debug.Log("generating world");
    Physics.gravity = new Vector3(0, 0, 1f);
    this.mapGenerationSettings = MapGenerationSettings.loadSettings(applicationPersistentPath);
    this.mapData = MapData.loadData(applicationPersistentPath);
    UIController.getInstance().updateSettingsPanel(this.mapGenerationSettings, 0);
    Map map = MapGenerator.generateMap(0, mapGenerationSettings, mapData, applicationPersistentPath, this.randomGenerator);
    //UIController.getInstance().setCameraPosition(HexMathHelper.hexToWorldCoords(MapGenerator.mapCenter(this.mapGenerationSettings), MeshMapController.getInstance().getHexSize()));
    MeshMapController.getInstance().map = map;
    MeshMapController.getInstance().drawMap();

    this.partyGenerationSettings = PartyGenerationSettings.loadSettings(applicationPersistentPath);
    Party party = PartyGenerator.getInstance().generateParty(applicationPersistentPath, this.partyGenerationSettings);
    PartyController.getInstance().party = party;
    PartyController.getInstance().setInitialPosition(MapGenerator.mapCenter(this.mapGenerationSettings));
   
    UIController.getInstance().showPartyInfo();

    UIController.getInstance().startingMenuPanel.SetActive(false);
    UIController.getInstance().inGamePanel.SetActive(true);

    this.currentTime = this.partyGenerationSettings.nightTime;

  }

  public void moveParty(int movementCost, Vector2 hexCoordinates)
  {
    for (int i = 0; i < movementCost; i++)
    {
      this.passTurn();
    }
    PartyController.getInstance().setPosition(hexCoordinates);
  }

  public void passTurn()
  {
    this.currentTime++;
    UIController.getInstance().updateTimePanel(this.currentTime, this.partyGenerationSettings.dayTime, this.partyGenerationSettings.nightTime);
  }

  public void regenerateMap()
  {
    Map map = MapGenerator.generateMap(0, mapGenerationSettings, mapData, applicationPersistentPath, this.randomGenerator);
    MeshMapController.getInstance().regenerateMap(map);
  }
	
	// Update is called once per frame
	void Update () {
		
	}
}
