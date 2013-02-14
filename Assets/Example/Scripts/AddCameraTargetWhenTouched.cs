using UnityEngine;
using System.Collections;

public class AddCameraTargetWhenTouched : MonoBehaviour {
	public CameraController2D cameraController;
	public Transform target;
	public float moveSpeed;
	public bool removeTargetAfterDelay;
	public float delay = 5;
	public float revertMoveSpeed;

	public bool triggerTweenAtTarget;
	public GameObject tweenTarget;
	public string tweenName;
	
	void Start() {
		if(cameraController == null) {
			cameraController = Camera.main.GetComponent<CameraController2D>();
		}
	}

	void OnTriggerEnter() {

		if(removeTargetAfterDelay) {
			cameraController.AddTarget(target, moveSpeed, delay, revertMoveSpeed);
		}
		else {
			cameraController.AddTarget(target, moveSpeed);
		}

		if(triggerTweenAtTarget) {
			Debug.Log("Adding callback");
			cameraController.OnNewTargetReached += StartTween;
			Debug.Log("callback null? " + (cameraController.OnNewTargetReached == null));
		}
	}

	void StartTween() {
		iTweenEvent.GetEvent(tweenTarget, tweenName).Play();
		cameraController.OnNewTargetReached -= StartTween;
	}
}