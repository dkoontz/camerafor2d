using UnityEngine;
using System.Collections;
using GoodStuff.NaturalLanguage;

public class FocusPointOfInterest : MonoBehaviour {

	public Transform player;
	public float midpointFocusDistance;
	public float exclusiveFocusDistance;
	public float cameraMoveSpeedWhileFocusing;

	Transform focusTarget;
	float squareMidpointFocusDistance;
	float squareExclusiveFocusDistance;
	bool controllingCamera;
	float originalMoveSpeed; // stores the original move speed of the camera so we can restore it when leaving the focus distance

	void Start() {
		focusTarget = new GameObject("_PointOfInterestFocusTarget").transform;
		squareMidpointFocusDistance = Mathf.Pow(midpointFocusDistance, 2);
		squareExclusiveFocusDistance = Mathf.Pow(exclusiveFocusDistance, 2);
	}

	void Update() {
		var vectorToPlayer = player.position - transform.position;
		var distanceSquared = vectorToPlayer.sqrMagnitude;
//		if(distanceSquared < squareExclusiveFocusDistance) {
////			focusTarget.position = (transform.position - focusTarget.position).normalized * Time.deltaTime * Camera.main.GetComponent<CameraController2D>().maxMoveSpeedPerSecond;
//			focusTarget.position = transform.position;
//			StartControllingCamera();
//		}
//		else 
		if(distanceSquared < squareMidpointFocusDistance) {
			var percentOfDistance = (distanceSquared / squareMidpointFocusDistance).MapToRange(.1f, 1, 0, 1, true);
//			var percentOfDistance = distanceSquared / squareMidpointFocusDistance;
			focusTarget.position = transform.position + (vectorToPlayer * percentOfDistance);
			StartControllingCamera();
		}
		else {
			StopControllingCamera();
		}
	}

	void StartControllingCamera() {
		if(!controllingCamera) {
			controllingCamera = true;
			var cameraController = Camera.main.GetComponent<CameraController2D>();
			originalMoveSpeed = cameraController.maxMoveSpeedPerSecond;
			cameraController.maxMoveSpeedPerSecond = cameraMoveSpeedWhileFocusing;
			cameraController.AddTarget(focusTarget);
		}
	}

	void StopControllingCamera() {
		if(controllingCamera) {
			controllingCamera = false;
			var cameraController = Camera.main.GetComponent<CameraController2D>();
			cameraController.maxMoveSpeedPerSecond = originalMoveSpeed;
			Camera.main.GetComponent<CameraController2D>().RemoveCurrentTarget();
		}
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, midpointFocusDistance);
	}
}