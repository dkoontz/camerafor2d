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
		var movementVector = (Input.GetAxis(horizontalInput) * Vector3.right * moveSpeed * Time.deltaTime) + (Input.GetAxis(verticalInput) * Vector3.forward * moveSpeed * Time.deltaTime);
//		var movementVector = Vector3.right * Time.deltaTime;
		characterController.Move(movementVector);
	}
}
