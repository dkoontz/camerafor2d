using UnityEngine;
using System.Collections;

public class ToggleColliderStatusOfObjectsWhenTouched : MonoBehaviour {
	public GameObject[] objects;

	void OnTriggerEnter() {
		if(objects[0] != null && objects[0].collider.enabled) {
			foreach(var target in objects) {
				target.collider.enabled = false;
			}
		}
		else {
			foreach(var target in objects) {
				target.collider.enabled = true;
			}
		}
	}
}
