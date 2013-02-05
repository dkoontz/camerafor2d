using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

	public string horizontalInput = "Horizontal";
	public string verticalInput = "Vertical";
	public float moveSpeed;

	CharacterController characterController;

	void Start() {
		characterController = GetComponent<CharacterController>();
	}

	void Update() {
//		if(Input.GetKeyDown(KeyCode.W)) {
//			characterController.Move(Vector3.forward * 2);
//		}
		var movementVector = (Input.GetAxis(horizontalInput) * Vector3.right * moveSpeed * Time.deltaTime) + (Input.GetAxis(verticalInput) * Vector3.forward * moveSpeed * Time.deltaTime);
		characterController.Move(movementVector);
	}
}
