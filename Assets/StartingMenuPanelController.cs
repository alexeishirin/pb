using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartingMenuPanelController : MonoBehaviour {

  public Image leftChevron;
  public Image rightChevron;

  private int outsideScreenY = 300;

  public void Start()
  {
    this.highlightMenuItem(null);
  }

  public void highlightMenuItem(RectTransform menuItem)
  {
    if (menuItem == null)
    {
      leftChevron.gameObject.SetActive(false);
      rightChevron.gameObject.SetActive(false);
    }
    else
    {
      leftChevron.gameObject.SetActive(true);
      rightChevron.gameObject.SetActive(true);
      leftChevron.rectTransform.position = new Vector3(leftChevron.rectTransform.position.x, menuItem.position.y, leftChevron.rectTransform.position.z);
      rightChevron.rectTransform.position = new Vector3(rightChevron.rectTransform.position.x, menuItem.position.y, rightChevron.rectTransform.position.z);
    }
  }
	
}
