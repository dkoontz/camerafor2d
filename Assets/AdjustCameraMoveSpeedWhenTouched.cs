using UnityEngine;
using System.Collections;

public class AdjustCameraMoveSpeedWhenTouched : MonoBehaviour {

	public float cameraMoveSpeed;

	void OnTriggerEnter() {
		Camera.main.GetComponent<CameraController2D>().maxMoveSpeedPerSecond = cameraMoveSpeed;
	}
}