using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

	public string horizontalInput = "Horizontal";
	public string verticalInput = "Vertical";
	public float moveSpeed;
	public CameraController2D cameraController;
	public float lookaheadScale = 0;

	// Very simple sprite system to show the player character
	public Renderer spriteRenderer;
	public int spriteSheetCellsWide;
	public int spriteSheetCellsHigh;
	public float animationFrameDelay;
	public Vector2[] upFrames;
	public Vector2[] downFrames;
	public Vector2[] leftFrames;
	public Vector2[] rightFrames;

	CharacterController characterController;
	int currentAnimationFrameIndex;
	float cellWidth;
	float cellHeight;
	float changeToNextFrameAt;

	void Start() {
		if(cameraController == null) {
			cameraController = Camera.main.GetComponent<CameraController2D>();
		}
		characterController = GetComponent<CharacterController>();

		cellWidth = 1f / spriteSheetCellsWide;
		cellHeight = 1f / spriteSheetCellsHigh;
	}

	void Update() {
		var inputVector = (Input.GetAxis(horizontalInput) * Vector3.right) + (Input.GetAxis(verticalInput) * Vector3.forward);
		var movementVector = inputVector * moveSpeed * Time.deltaTime;
		characterController.Move(movementVector);
		cameraController.AddInfluence(inputVector * lookaheadScale);

		if(Time.time >= changeToNextFrameAt && inputVector.magnitude > 0) {
			changeToNextFrameAt = Time.time + animationFrameDelay;
			currentAnimationFrameIndex++;
		}

		// determine facing
		if(inputVector.x > 0) {
			spriteRenderer.material.mainTextureOffset = ConvertPositionToOffset(rightFrames[currentAnimationFrameIndex % rightFrames.Length]);
		}
		else if(inputVector.x < 0) {
			spriteRenderer.material.mainTextureOffset = ConvertPositionToOffset(leftFrames[currentAnimationFrameIndex % leftFrames.Length]);
		}
		else if(inputVector.z > 0) {
			spriteRenderer.material.mainTextureOffset = ConvertPositionToOffset(upFrames[currentAnimationFrameIndex % upFrames.Length]);
		}
		else if(inputVector.z < 0) {
			spriteRenderer.material.mainTextureOffset = ConvertPositionToOffset(downFrames[currentAnimationFrameIndex % downFrames.Length]);
		}
	}

	// Converts a position in the sprite sheet such as (2, 1) to a texture offset such as (.2222, .75)
	Vector2 ConvertPositionToOffset(Vector2 position) {
		return new Vector2(position.x * cellWidth, 1 - ((position.y + 1) * cellHeight));
	}
}