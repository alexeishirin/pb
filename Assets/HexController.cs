using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;


public class HexController : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler {

	public TextMesh hexText;
  public Hex hex;

	void Start () {

	}

	void Update () {

	}

	public void OnPointerDown( PointerEventData eventData ){
	}

	public void OnPointerUp( PointerEventData eventData ){
	}

	public void OnPointerClick( PointerEventData eventData ){
    Debug.Log("click");
  }

  public void OnPointerEnter ( PointerEventData eventData)
  {
    UIController.getInstance().showHexInfo(this.hex);
  }
}
