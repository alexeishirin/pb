using UnityEngine;
using System.Collections;

public class PartyController : MonoBehaviour
{
  float currentLerpTime = 1.0f;
  float lerpTime = 1.0f;

  Vector3 destination = new Vector3(0.0f, 0.0f, -1);
  Vector3 lerpStartPosition = new Vector3(0.0f, 0.0f, -1);

  public MeshMapController mapController;
  public GameStateManager gameStateManager;

  public Party party;
  public Vector2 partyPosition = new Vector2(0,0);
  // Use this for initialization
  void Start()
  {
    lerpStartPosition = destination = this.transform.position;
  }

  private void Update()
  {
    currentLerpTime += Time.deltaTime;
    if (currentLerpTime > lerpTime)
    {
      currentLerpTime = lerpTime;
    }
    if (currentLerpTime != lerpTime)
    {
      float t = currentLerpTime / lerpTime;
      t = t * t * t * (t * (6f * t - 15f) + 10f);
      this.transform.position = Vector3.Lerp(this.lerpStartPosition, this.destination, t);
    }
  }

  public void setPosition(Vector2 newHexPosition)
  {
    this.partyPosition = newHexPosition;
    Vector3 newPosition = HexMathHelper.hexToWorldCoords(this.partyPosition, MeshMapController.getInstance().getHexSize());
    newPosition.z = this.transform.position.z;
    this.setNewDestination(newPosition);
  }

  public void setInitialPosition(Vector2 newHexPosition)
  {
    this.partyPosition = newHexPosition;
    Vector3 newPosition = HexMathHelper.hexToWorldCoords(this.partyPosition, MeshMapController.getInstance().getHexSize());
    newPosition.z = this.transform.position.z;
    this.transform.position = newPosition;
    destination = lerpStartPosition = newPosition;
  }

  public void setNewDestination(Vector2 newDestination)
  {
    this.destination = new Vector3(newDestination.x, newDestination.y, this.transform.position.z);
    this.lerpStartPosition = this.transform.position;
    Debug.DrawLine(lerpStartPosition, destination, Color.green, 100.0f);
    this.currentLerpTime = 0f;
  }

  public int calculateMovementCost(Vector2 newPosition)
  {
    Hex targetHex = this.mapController.map.getHex(newPosition);
    int movementCost = this.gameStateManager.partyGenerationSettings.minimalMovement;
    Debug.Log("Initial movement cost:" + movementCost);
    int movementPenaltyValue = 0;
    foreach (MovementPenalty movementPenalty in this.gameStateManager.partyGenerationSettings.movementPenalty)
    {
      movementPenaltyValue += movementPenalty.calculatePenalty(targetHex);
    }
    Debug.Log("Movement penalty:" + movementPenaltyValue);
    if (movementPenaltyValue > 0)
    {
      Character pathFinderCharacter = null;
      int maxPenaltyNegation = 0;
      foreach (Character character in this.party.characters)
      {
        int characterPenaltyNegation = 0;
        Skill pathFinderSkill = character.skills.Find(characterSkill => characterSkill.name == "Pathfinder");
        if (pathFinderSkill == null)
        {
          continue;
        }

        characterPenaltyNegation += pathFinderSkill.points;
        foreach (Ability ability in character.abilities.FindAll(ability => ability.parentSkill == "Pathfinder"))
        {
          if (targetHex.hasAnyTag(ability.targetHexTags))
          {
            characterPenaltyNegation += ability.skillBonus;
          }
        }
        if(characterPenaltyNegation > maxPenaltyNegation)
        {
          pathFinderCharacter = character;
          maxPenaltyNegation = characterPenaltyNegation;
        }
      }
      if(pathFinderCharacter != null)
      {
        Debug.Log("Path Finder: " + pathFinderCharacter.ToString());
        movementPenaltyValue -= maxPenaltyNegation;
        if(movementPenaltyValue < 0)
        {
          movementPenaltyValue = 0;
        }
      }
    }

    movementCost += movementPenaltyValue;

    return movementCost;
  }

  public static PartyController getInstance()
  {
    return GameObject.FindGameObjectWithTag("Party").GetComponent<PartyController>();
  }

}
