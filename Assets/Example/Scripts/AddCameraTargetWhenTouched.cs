using UnityEngine;
using System.Collections;

public class AddCameraTargetWhenTouched : MonoBehaviour {

	public Transform target;
	public float moveSpeed;
	public bool removeTargetAfterDelay;
	public float delay = 5;
	public bool snapBackAfterDelay;

	void OnTriggerEnter() {
		if(removeTargetAfterDelay) {
			Camera.main.GetComponent<CameraController2D>().AddTarget(target, moveSpeed, delay, snapBackAfterDelay);
		}
		else {
			Camera.main.GetComponent<CameraController2D>().AddTarget(target, moveSpeed);
		}
	}
}