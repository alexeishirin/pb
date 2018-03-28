using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HexHighlightController : MonoBehaviour, IPointerClickHandler
{
  public TextMesh movementCostText;

  public int movementCost;
  public Vector2 movementTarget;

  public GameStateManager gameStateManager;

  public void showMovementCost(int movementCost)
  {
    this.movementCostText.text = movementCost.ToString();
  }

  public void highlightHex(Vector2 hexCoordinates)
  {
    float hexSize = MeshMapController.getInstance().getHexSize();
    Vector2 newHighlightPosition = HexMathHelper.hexToWorldCoords(hexCoordinates, hexSize);
    this.transform.position = new Vector3(newHighlightPosition.x, newHighlightPosition.y, this.transform.position.z);
    this.gameObject.SetActive(true);
    int movementCost = PartyController.getInstance().calculateMovementCost(hexCoordinates);
    this.showMovementCost(movementCost);

    this.movementCost = movementCost;
    this.movementTarget = hexCoordinates;
  }

  public void hideHighlight()
  {
    this.gameObject.SetActive(false);
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    Debug.Log("MOVE!");
    gameStateManager.moveParty(this.movementCost, this.movementTarget);
  }
}