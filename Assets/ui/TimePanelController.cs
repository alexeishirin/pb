using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimePanelController : MonoBehaviour {
  public Text timeText;

  public void updateTime(int newTime, int dayLength, int nightLength)
  {
    int currentDay = newTime / (dayLength + nightLength);
    currentDay++;//starting with one
    int currentTime = newTime % (dayLength + nightLength);
    int displayedTime = currentTime < nightLength ? currentTime : currentTime - nightLength;
    string dayOrNightLabel = currentTime < nightLength ? "Night" : "Day";
    this.timeText.text = displayedTime + " o'clock on " + dayOrNightLabel + " " + currentDay;
  }
	
}
