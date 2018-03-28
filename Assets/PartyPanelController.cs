using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyPanelController : MonoBehaviour {

  public Text partyText;

  public void showPartyInfo()
  {
    this.partyText.text = PartyController.getInstance().party.ToString();
  }
}
