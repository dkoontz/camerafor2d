using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

	public string horizontalInput = "Horizontal";
	public string verticalInput = "Vertical";
	public float moveSpeed;
	public CameraController2D camera;
	public float lookaheadScale = 0;

	CharacterController characterController;

	void Start() {
		characterController = GetComponent<CharacterController>();
	}

	void Update() {
//		Debug.Log("horizontal: " + Input.GetAxis(horizontalInput) + ", vertical: " + Input.GetAxis(verticalInput));
		var movementVector = ((Input.GetAxis(horizontalInput) * Vector3.right) + (Input.GetAxis(verticalInput) * Vector3.forward)) * moveSpeed * Time.deltaTime;
		characterController.Move(movementVector);
		camera.AddInfluence(movementVector.normalized * lookaheadScale);
	}
}
