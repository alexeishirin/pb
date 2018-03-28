using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class PartyGenerationSettings
{
  public static string PARTY_GENERATION_SETTINGS_PERSIST_PATH = "partySettings";

  public int maxPartyMembers = 6;
  public int maxActiveArchetypes = 4;
  public int archetypeLevelCap = 4;
  public int startingArchetypePoints = 12;
  public int minPassiveReserve = 4;
  public int maxPassiveReserve = 4;
  public int skillPromotionChance = 100;
  public int maxPromotedSkills = 2;
  public int maxPromotionBonus = 2;
  public int archetypeDrop = 1;
  public int dropValue = 1;
  public int abilityChance = 30;
  public int maxAbilities = 2;
  public int minimalMovement = 1;
  public int dayTime = 8;
  public int nightTime = 8;
  public int decisionCost = 3;
  public List<MovementPenalty> movementPenalty;

  public static PartyGenerationSettings loadSettings(string applicationPersistentPath)
  {
    string partyGenerationSettingsJson = DataLoader.loadFile(applicationPersistentPath, PARTY_GENERATION_SETTINGS_PERSIST_PATH);
    Debug.Log(partyGenerationSettingsJson);
    return JsonUtility.FromJson<PartyGenerationSettings>(partyGenerationSettingsJson);
  }

  public static void saveSettings(string applicationPersistentPath, MapGenerationSettings mapGenerationSettings)
  {
    Debug.Log("saving...");
    string settingsJson = JsonUtility.ToJson(mapGenerationSettings);
    File.WriteAllText(applicationPersistentPath + "/" + PARTY_GENERATION_SETTINGS_PERSIST_PATH + ".json", settingsJson);
    Debug.Log("saved");
  }

  public override string ToString()
  {
    return JsonUtility.ToJson(this);
  }

}
