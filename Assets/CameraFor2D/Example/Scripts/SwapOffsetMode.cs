using UnityEngine;
using System.Collections;

public class SwapOffsetMode : MonoBehaviour {
	public CameraController2D cameraController;
	public Transform player;
	public Transform offsetTarget;

	bool usingTarget = true;

	void Start() {
		if(cameraController == null) {
			cameraController = Camera.main.GetComponent<CameraController2D>();
		}
	}

	void OnTriggerEnter() {
		usingTarget = !usingTarget;

		if(usingTarget) {
			cameraController.GetComponent<AddInfluence>().enabled = false;
			cameraController.SetTarget(offsetTarget);
		}
		else {
			cameraController.SetTarget(player);
			cameraController.GetComponent<AddInfluence>().enabled = true;
		}

	}
}
