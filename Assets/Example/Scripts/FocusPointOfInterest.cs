using UnityEngine;
using System.Collections;
using GoodStuff.NaturalLanguage;

public class FocusPointOfInterest : MonoBehaviour {
	
	public CameraController2D camera;
	public GameObject target;
	public float focusDistance;
	public float exclusiveFocusPercentage = .25f;

	float focusDistanceSquared;
	Vector3 influencePoint;

	void Start() {
		focusDistanceSquared = Mathf.Pow(focusDistance, 2);
	}

	void Update() {
		var vectorToTarget = target.transform.position - transform.position;
		var distanceSquared = vectorToTarget.sqrMagnitude;

		if(distanceSquared < focusDistanceSquared) {
			var percentOfDistance = (vectorToTarget.magnitude / focusDistance).MapToRange(exclusiveFocusPercentage, 1, 0, 1, true);
			camera.AddInfluence(-vectorToTarget * (1 - percentOfDistance));
		}
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, focusDistance);
	}
}