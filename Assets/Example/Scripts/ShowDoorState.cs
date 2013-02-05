using UnityEngine;
using System.Collections;

public class ShowDoorState : MonoBehaviour {

	public Collider sampleDoorCollider;

	TextMesh textMesh;

	void Start () {
		textMesh = GetComponent<TextMesh>();
	}
	
	void Update () {
		textMesh.text = "Doors currently: " + (sampleDoorCollider.enabled ? "Enabled" : "Disabled");
	}
}
