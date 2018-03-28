using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour {

  public MapGenerationSettings mapGenerationSettings;
  private string persistentPath;
  public Text pathToFile;
  public InputField mapHeightInputField;
  public InputField mapWidthInputField;
  public InputField mapSeedInputField;
  public InputField minRegionSizeInputField;
  public InputField maxRegionSizeInputField;
  public InputField clustersPercentInputField;
  public InputField adjacencyBonusInputField;
  public InputField minMiddleClimatePercentInputField;
  public InputField maxMiddleClimatePercentInputField;

  // Use this for initialization
  void Start () {
    persistentPath = Application.persistentDataPath;
    pathToFile.text = persistentPath + MapGenerationSettings.MAP_GENERATION_SETTINGS_PERSIST_PATH;
  }
	
	// Update is called once per frame
	void Update () {
		
	}

  public void updatePanel(MapGenerationSettings mapGenerationSettings, int mapSeed)
  {
    Debug.Log(this.mapGenerationSettings.mapHeight);
    this.mapGenerationSettings = mapGenerationSettings;
    this.mapHeightInputField.text = this.mapGenerationSettings.mapHeight.ToString();
    this.mapWidthInputField.text = this.mapGenerationSettings.mapWidth.ToString();
    this.mapSeedInputField.text = mapSeed.ToString();
    this.minRegionSizeInputField.text = this.mapGenerationSettings.minRegionSize.ToString();
    this.maxRegionSizeInputField.text = this.mapGenerationSettings.maxRegionSize.ToString();
    this.clustersPercentInputField.text = this.mapGenerationSettings.terrainClustersPercent.ToString();
    this.adjacencyBonusInputField.text = this.mapGenerationSettings.terrainAdjucencyPercentBoost.ToString();
    this.minMiddleClimatePercentInputField.text = this.mapGenerationSettings.middleClimateStripeMinHeightPercent.ToString();
    this.maxMiddleClimatePercentInputField.text = this.mapGenerationSettings.middleClimateStripeMaxHeightPercent.ToString();
  }

  public void saveSettings()
  {
    MapGenerationSettings.saveSettings(persistentPath, this.mapGenerationSettings);
  }

  public void restoreDefaults()
  {
    updatePanel(MapGenerationSettings.loadSettings(persistentPath), 0);
  }

  public void setMapHeight(string newMapHeight)
  {
    int number;
    bool isInt = int.TryParse(newMapHeight, out number);
    if(isInt)
    {
      this.mapGenerationSettings.mapHeight = number;
    }
  }

  public void setMapWidth(string newMapWidth)
  {
    int number;
    bool isInt = int.TryParse(newMapWidth, out number);
    if (isInt)
    {
      this.mapGenerationSettings.mapWidth = number;
    }
  }

  public void setMinRegionSize(string newMinRegionSize)
  {
    int number;
    bool isInt = int.TryParse(newMinRegionSize, out number);
    if (isInt)
    {
      this.mapGenerationSettings.minRegionSize = number;
    }
  }

  public void setMaxRegionSize(string newMaxRegionSize)
  {
    int number;
    bool isInt = int.TryParse(newMaxRegionSize, out number);
    if (isInt)
    {
      this.mapGenerationSettings.maxRegionSize = number;
    }
  }

  public void setTerrainClustersPercent(string newTerrainClustersPercent)
  {
    int number;
    bool isInt = int.TryParse(newTerrainClustersPercent, out number);
    if (isInt)
    {
      this.mapGenerationSettings.terrainClustersPercent = number;
    }
  }

  public void setTerrainAdjucencyPercentBoost(string newTerrainAdjucencyPercentBoost)
  {
    int number;
    bool isInt = int.TryParse(newTerrainAdjucencyPercentBoost, out number);
    if (isInt)
    {
      this.mapGenerationSettings.terrainAdjucencyPercentBoost = number;
    }
  }

  public void setMiddleClimateStripeMinHeightPercent(string newMiddleClimateStripeMinHeightPercent)
  {
    int number;
    bool isInt = int.TryParse(newMiddleClimateStripeMinHeightPercent, out number);
    if (isInt)
    {
      this.mapGenerationSettings.middleClimateStripeMinHeightPercent = number;
    }
  }

  public void setMiddleClimateStripeMaxHeightPercent(string newMiddleClimateStripeMaxHeightPercent)
  {
    int number;
    bool isInt = int.TryParse(newMiddleClimateStripeMaxHeightPercent, out number);
    if (isInt)
    {
      this.mapGenerationSettings.middleClimateStripeMaxHeightPercent = number;
    }
  }

  public void setMapSeed(string newMapSeed)
  {
    int number;
    bool isInt = int.TryParse(newMapSeed, out number);
    if (isInt)
    {
      MeshMapController.getInstance().specifiedMapSeed = number;
    } else
    {
      MeshMapController.getInstance().specifiedMapSeed = 0;
    }
  }
}
