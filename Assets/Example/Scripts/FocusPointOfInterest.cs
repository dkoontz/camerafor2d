using UnityEngine;
using System.Collections;
using GoodStuff.NaturalLanguage;

public class FocusPointOfInterest : MonoBehaviour {
	
	public CameraController2D camera;
	public GameObject target;
	public float midpointFocusDistance;
//	public float exclusiveFocusDistance;
//	public float cameraMoveSpeedWhileFocusing;

	float squareMidpointFocusDistance;
//	float squareExclusiveFocusDistance;
//	bool controllingCamera;
//	float originalMoveSpeed; // stores the original move speed of the camera so we can restore it when leaving the focus distance
	Vector3 influence;

	void Start() {
		squareMidpointFocusDistance = Mathf.Pow(midpointFocusDistance, 2);
//		squareExclusiveFocusDistance = Mathf.Pow(exclusiveFocusDistance, 2);
	}

	void Update() {
		var vectorToTarget = target.transform.position - transform.position;
		var distanceSquared = vectorToTarget.sqrMagnitude;
//		if(distanceSquared < squareExclusiveFocusDistance) {
////			focusTarget.position = (transform.position - focusTarget.position).normalized * Time.deltaTime * Camera.main.GetComponent<CameraController2D>().maxMoveSpeedPerSecond;
//			focusTarget.position = transform.position;
//			StartControllingCamera();
//		}
//		else 
		if(distanceSquared < squareMidpointFocusDistance) {
			var percentOfDistance = (distanceSquared / squareMidpointFocusDistance).MapToRange(.1f, 1, 0, 1, true);
//			var percentOfDistance = distanceSquared / squareMidpointFocusDistance;
//			influence = (transform.position - (transform.position + vectorToTarget)) * (1 - percentOfDistance);
			influence = -vectorToTarget * (1 - percentOfDistance);
//			Debug.Log("percent: " + percentOfDistance + ", influence: " + influence + ", distance: " + influence.magnitude);
			camera.AddInfluence(influence);
//			StartControllingCamera();
		}
		else {
//			StopControllingCamera();
		}
	}

//	void StartControllingCamera() {
//		if(!controllingCamera) {
//			controllingCamera = true;
//			var cameraController = Camera.main.GetComponent<CameraController2D>();
//			originalMoveSpeed = cameraController.maxMoveSpeedPerSecond;
//			cameraController.maxMoveSpeedPerSecond = cameraMoveSpeedWhileFocusing;
//		}
//	}
//
//	void StopControllingCamera() {
//		if(controllingCamera) {
//			controllingCamera = false;
//			var cameraController = Camera.main.GetComponent<CameraController2D>();
//			cameraController.maxMoveSpeedPerSecond = originalMoveSpeed;
//			Camera.main.GetComponent<CameraController2D>().RemoveCurrentTarget();
//		}
//	}

	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, midpointFocusDistance);
		if(Application.isPlaying) {
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(target.transform.position + influence, .1f);
		}
	}
}