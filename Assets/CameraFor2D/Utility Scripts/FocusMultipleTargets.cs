using UnityEngine;
using System.Collections;

// Use this script to add multiple targets to your camera on start
public class FocusMultipleTargets : MonoBehaviour {

	// The targets you want to be focused initially
	public Transform[] targets;
	// Optional, will default to the main camera
	public Camera camera;

	void Start () {
		if (camera == null) {
			camera = Camera.main;
		}

		var controller = camera.GetComponent<CameraController2D>();
		controller.RemoveAllTargets();
		controller.AddTarget(targets);
	}
}