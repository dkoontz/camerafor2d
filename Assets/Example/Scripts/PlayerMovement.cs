using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

	public string horizontalInput = "Horizontal";
	public string verticalInput = "Vertical";
	public float moveSpeed;
	public CameraController2D cameraController;
	public float lookaheadScale = 0;

	CharacterController characterController;

	void Start() {
		if(cameraController == null) {
			cameraController = Camera.main.GetComponent<CameraController2D>();
		}
		characterController = GetComponent<CharacterController>();
	}

	void Update() {
		var inputVector = (Input.GetAxis(horizontalInput) * Vector3.right) + (Input.GetAxis(verticalInput) * Vector3.forward);
		var movementVector = inputVector * moveSpeed * Time.deltaTime;
		characterController.Move(movementVector);
		cameraController.AddInfluence(inputVector * lookaheadScale);
	}
}
