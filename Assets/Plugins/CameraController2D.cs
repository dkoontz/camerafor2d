using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoodStuff.NaturalLanguage;

public class CameraController2D : MonoBehaviour {
	public enum MovementAxis {
		XY,
		XZ,
		YZ
	}

	class OffsetData {
		public Vector3 Vector { get; set; }
		public Vector3 NormalizedVector { get; set; }
		public float DistanceFromCenterpoint { get; set; }
	}
	
	public MovementAxis axis = MovementAxis.XZ;
	public LayerMask cameraBumperLayers;
	public Transform target;
	public float distance;

	public bool drawDebugLines;

	System.Func<Vector3> IdealCameraPosition;
	System.Func<Vector3> HeightOffset;
	System.Func<Vector3, Vector3> GetHorizontalComponent;
	System.Func<Vector3, Vector3> GetVerticalComponent;

	// These points are used to determine if the camera's ideal position lies within a
	// camera bumper collider and thus we must stop the screen from moving.  They are
	// calculated as points that are at the four corners of the screen at the target
	// object's height.  In this way these points are checked at the same "height" as
	// the player, allowing you to push up against a wall, but then go back, walk up
	// stairs and walk past the collider since it will exist at a different height
	// than the target is now at.
	OffsetData[] horizontalRaycastPointOffsets = new OffsetData[2];
	OffsetData[] verticalRaycastPointOffsets = new OffsetData[2];

	void Start() {
		switch(axis) {
//		case MovementAxis.XY:
//			HeightOffset = () => Vector3.forward * distance;
//			IdealCameraPosition = () => target.position - HeightOffset();
//			break;
		case MovementAxis.XZ:
			HeightOffset = () => -Vector3.up * distance;
			GetHorizontalComponent = (vector) => new Vector3(vector.x, 0, 0);
			GetVerticalComponent = (vector) => new Vector3(0, 0, vector.z);

			break;
//		case MovementAxis.YZ:
//			HeightOffset = () => Vector3.right * distance;
//			IdealCameraPosition = () => target.position + HeightOffset();
//			break;
		}

		CalculateScreenBounds();
		IdealCameraPosition = () => target.position - HeightOffset();

		transform.position = IdealCameraPosition();
	}
	
	void LateUpdate() {
		var idealPosition = IdealCameraPosition();
		var distanceToIdealPosition = (idealPosition - transform.position).magnitude;

		if(distanceToIdealPosition > 0) {
			var shortestDistance = distanceToIdealPosition;
			var idealCenterPoint = idealPosition + HeightOffset();
			var distanceReductionDueToCollision = CalculateDistanceReduction(horizontalRaycastPointOffsets, idealCenterPoint);
			var vectorToIdealPosition = idealPosition - transform.position;
			var horizontalVector = GetHorizontalComponent(vectorToIdealPosition);
			var idealHorizontalDistance = horizontalVector.magnitude;
			horizontalVector = horizontalVector.normalized * Mathf.Max(idealHorizontalDistance - distanceReductionDueToCollision, 0);

			distanceReductionDueToCollision = CalculateDistanceReduction(verticalRaycastPointOffsets, idealCenterPoint);
			var verticalVector = GetVerticalComponent(vectorToIdealPosition);
			var idealVerticalDistance = verticalVector.magnitude;
			verticalVector = verticalVector.normalized * Mathf.Max(idealVerticalDistance - distanceReductionDueToCollision, 0);

			transform.Translate(horizontalVector + verticalVector, Space.World);
		}
	}

	void CalculateScreenBounds() {
		System.Func<Vector3, OffsetData> AddRaycastOffsetPoint = (viewSpacePoint) => {
			var vectorToOffset = camera.ViewportToWorldPoint(viewSpacePoint) - transform.position;
			return new OffsetData { Vector = vectorToOffset, NormalizedVector = vectorToOffset.normalized, DistanceFromCenterpoint = vectorToOffset.magnitude };
		};

		verticalRaycastPointOffsets[0] = AddRaycastOffsetPoint(new Vector3(0.5f, 0));
		verticalRaycastPointOffsets[1] = AddRaycastOffsetPoint(new Vector3(0.5f, 1));
		horizontalRaycastPointOffsets[0] = AddRaycastOffsetPoint(new Vector3(0, 0.5f));
		horizontalRaycastPointOffsets[1] = AddRaycastOffsetPoint(new Vector3(1, 0.5f));
	}

	float CalculateDistanceReduction(OffsetData[] offsets, Vector3 idealCenterPoint) {
		RaycastHit hitInfo;
		var distanceReductionDueToCollision = 0f;

		offsets.Each(offset => {
			if(Physics.Raycast(idealCenterPoint, offset.NormalizedVector, out hitInfo, offset.DistanceFromCenterpoint, cameraBumperLayers)) {
				var collisionDistance = offset.DistanceFromCenterpoint - hitInfo.distance;
				if(collisionDistance > distanceReductionDueToCollision) distanceReductionDueToCollision = collisionDistance;
				if(drawDebugLines) Debug.DrawLine(idealCenterPoint, idealCenterPoint + (offset.NormalizedVector * hitInfo.distance), Color.red);
			}
			else if(drawDebugLines) Debug.DrawLine(idealCenterPoint, idealCenterPoint + offset.Vector, Color.green);
		});
		return distanceReductionDueToCollision;
	}
}
