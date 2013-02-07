using UnityEngine;
using System.Collections;

public class AddCameraTargetWhenTouched : MonoBehaviour {

	public Transform target;
	public float moveSpeed;
	public bool removeTargetAfterDelay;
	public float delay = 5;
	public float revertMoveSpeed;

	void OnTriggerEnter() {
		if(removeTargetAfterDelay) {
			Camera.main.GetComponent<CameraController2D>().AddTarget(target, moveSpeed, delay, revertMoveSpeed);
		}
		else {
			Camera.main.GetComponent<CameraController2D>().AddTarget(target, moveSpeed);
		}
	}
}