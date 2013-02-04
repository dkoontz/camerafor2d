using UnityEngine;
using System.Collections;

public class MovementTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		RaycastHit hitInfo;
		var movementVector = Vector3.right + (Vector3.forward * 0.2f).normalized;
		if(rigidbody.SweepTest(movementVector, out hitInfo, Time.deltaTime * 3)) {
			transform.Translate(movementVector * hitInfo.distance);
		}
		else transform.Translate(movementVector * Time.deltaTime * 3);

	}

	void OnCollisionEnter(Collision collision) {
		Debug.Log("Collision enter");
	}

	void OnCollisionExit(Collision collision) {
		Debug.Log("Collision exit");
	}

	void OnTriggerEnter(Collider collision) {
		Debug.Log("Trigger enter");
	}

	void OnTriggerExit(Collider collision) {
		Debug.Log("Trigger exit");
	}
}
