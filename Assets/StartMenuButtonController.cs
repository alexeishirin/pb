using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StartMenuButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
  public void OnPointerEnter(PointerEventData eventData)
  {
    UIController.getInstance().highlightMenuItem(this.GetComponent<RectTransform>());
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    UIController.getInstance().highlightMenuItem(null);
  }

  // Use this for initialization
  void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
