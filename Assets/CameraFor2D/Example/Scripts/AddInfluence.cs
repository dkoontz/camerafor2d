using UnityEngine;
using System.Collections;

public class AddInfluence : MonoBehaviour {
	public Vector3 influence;
	public CameraController2D cameraController;

	void Start() {
		if(cameraController == null) {
			cameraController = Camera.main.GetComponent<CameraController2D>();
		}
	}

	void Update () {
		cameraController.AddInfluence(influence);	
	}
}