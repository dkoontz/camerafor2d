using UnityEngine;
using System.Collections;

public class AddCameraTargetWhenTouched : MonoBehaviour {
	public CameraController2D camera;
	public Transform target;
	public float moveSpeed;
	public bool removeTargetAfterDelay;
	public float delay = 5;
	public float revertMoveSpeed;

	public bool triggerTweenAtTarget;
	public GameObject tweenTarget;
	public string tweenName;
	

	void OnTriggerEnter() {

		if(removeTargetAfterDelay) {
			camera.AddTarget(target, moveSpeed, delay, revertMoveSpeed);
		}
		else {
			camera.AddTarget(target, moveSpeed);
		}

		if(triggerTweenAtTarget) {
			Debug.Log("Adding callback");
			camera.OnNewTargetReached += StartTween;
			Debug.Log("callback null? " + (camera.OnNewTargetReached == null));
		}
	}

	void StartTween() {
		iTweenEvent.GetEvent(tweenTarget, tweenName).Play();
		camera.OnNewTargetReached -= StartTween;
	}
}