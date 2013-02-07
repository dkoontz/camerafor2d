using UnityEngine;
using System.Collections;

public class TeleportPlayer : MonoBehaviour {
	public Transform teleportTarget;
	public bool snapCameraToTarget = true;

	void OnTriggerEnter(Collider other) {
		if(other.tag == "Player") {
			other.transform.position = teleportTarget.position;
			other.transform.rotation = teleportTarget.rotation;
			if(snapCameraToTarget) {
				Camera.main.GetComponent<CameraController2D>().JumpToIdealPosition();
			}
		}
	}
}
