using UnityEngine;
using System.Collections;
using GoodStuff.NaturalLanguage;

public class ZoomCameraWhenTouched : MonoBehaviour {
	public CameraController2D cameraController;
	public float zoomAmount;

	void Start() {
		if(cameraController == null) {
			cameraController = Camera.main.GetComponent<CameraController2D>();
		}
	}

	void OnTriggerEnter() {
		iTween.ValueTo(gameObject, iTween.Hash("from", 1, "to", 3, "time", 2, "onupdate", "UpdateCameraZoom", "oncomplete", "ZoomDown"));
	}

	public void UpdateCameraZoom(float value) {
		cameraController.DistanceMultiplier = value;
	}

	public void ZoomDown() {
		iTween.ValueTo(gameObject, iTween.Hash("from", 3, "to", 1, "time", 2, "onupdate", "UpdateCameraZoom"));
	}
}