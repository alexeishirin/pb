using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour {

  public Camera characterCamera;

	void Update () {
    float turn = Input.GetAxis("Mouse X") * Time.deltaTime * 50f;
    float upDown = Input.GetAxis("Mouse Y") * Time.deltaTime * 40f;
    float move = Input.GetAxis("Vertical") * Time.deltaTime * 3f;
    float moveSideways = Input.GetAxis("Horizontal") * Time.deltaTime * 3f;

    transform.Rotate(0, turn, 0);
    //transform.Translate(moveSideways, 0, move);
    this.GetComponent<Rigidbody>().AddRelativeForce(new Vector3 (moveSideways, 0, move) * 15f);
    //characterCamera.transform.Rotate(Vector3.left, upDown);
    characterCamera.transform.RotateAround(this.transform.position, -transform.right, upDown);
  }
}
