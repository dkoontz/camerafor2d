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
		public Vector3 StartPointRelativeToCamera { get; set; }
		public Vector3 Vector { get; set; }
		public Vector3 NormalizedVector { get; set; }
		public float DistanceFromStartPoint { get; set; }
	}
	
	public MovementAxis axis = MovementAxis.XZ;
	public LayerMask cameraBumperLayers;
	public Transform[] targets;
	public float distance;

	public bool drawDebugLines;

	System.Func<Vector3> IdealCameraPosition;
	System.Func<Vector3> HeightOffset;
	System.Func<Vector3, Vector3> GetHorizontalComponent;
	System.Func<Vector3, Vector3> GetVerticalComponent;
	System.Func<Vector3, float> GetHorizontalValue;
	System.Func<Vector3, float> GetVerticalValue;
	
	OffsetData[] horizontalRaycastPointOffsets = new OffsetData[6];
	OffsetData[] verticalRaycastPointOffsets = new OffsetData[6];
	Stack<IEnumerable<Transform>> targetStack = new Stack<IEnumerable<Transform>>();

	public void AddTarget(Transform target) {
		AddTarget(new [] { target });
	}

	public void AddTarget(IEnumerable<Transform> targets) {
		targetStack.Push(targets);
	}

	void Start() {
		switch(axis) {
//		case MovementAxis.XY:
//			HeightOffset = () => Vector3.forward * distance;
//			IdealCameraPosition = () => target.position - HeightOffset();
//			break;
		case MovementAxis.XZ:
			HeightOffset = () => -Vector3.up * distance;
			GetHorizontalComponent = (vector) => new Vector3(vector.x, 0, 0);
			GetHorizontalValue = (vector) => vector.x;
			GetVerticalComponent = (vector) => new Vector3(0, 0, vector.z);
			GetVerticalValue = (vector) => vector.z;

			break;
//		case MovementAxis.YZ:
//			HeightOffset = () => Vector3.right * distance;
//			IdealCameraPosition = () => target.position + HeightOffset();
//			break;
		}
		AddTarget(targets);

		CalculateScreenBounds();
		IdealCameraPosition = () => {
			var minHorizontal = targetStack.Peek().Min(t => GetHorizontalValue(t.position));
			var maxHorizontal = targetStack.Peek().Max(t => GetHorizontalValue(t.position));
			var horizontalOffset = (maxHorizontal - minHorizontal) * 0.5f;
			var minVertical = targetStack.Peek().Min(t => GetVerticalValue(t.position));
			var maxVertical = targetStack.Peek().Max(t => GetVerticalValue(t.position));
			var verticalOffset = (maxVertical - minVertical) * 0.5f;
			return (GetHorizontalComponent(Vector3.one) * (minHorizontal + horizontalOffset)) + (GetVerticalComponent(Vector3.one) * (minVertical + verticalOffset)) - HeightOffset();
		};

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
//			Debug.Log("h: " + distanceReductionDueToCollision);

			distanceReductionDueToCollision = CalculateDistanceReduction(verticalRaycastPointOffsets, idealCenterPoint);
			var verticalVector = GetVerticalComponent(vectorToIdealPosition);
			var idealVerticalDistance = verticalVector.magnitude;
			verticalVector = verticalVector.normalized * Mathf.Max(idealVerticalDistance - distanceReductionDueToCollision, 0);
			Debug.Log("v: " + distanceReductionDueToCollision);

			if(drawDebugLines) Debug.DrawLine(transform.position, idealPosition, Color.blue);
			transform.Translate(horizontalVector + verticalVector, Space.World);
		}
	}

	void CalculateScreenBounds() {
		System.Func<Vector3, Vector3, OffsetData> AddRaycastOffsetPoint = (viewSpaceOrigin, viewSpacePoint) => {
			var origin = camera.ViewportToWorldPoint(viewSpaceOrigin);
			var vectorToOffset = camera.ViewportToWorldPoint(viewSpacePoint) - origin;
			Debug.Log("origin: " + origin + ", vectorToOffset: " + vectorToOffset);
			return new OffsetData { StartPointRelativeToCamera = origin - transform.position, Vector = vectorToOffset, NormalizedVector = vectorToOffset.normalized, DistanceFromStartPoint = vectorToOffset.magnitude };
		};

		horizontalRaycastPointOffsets[0] = AddRaycastOffsetPoint(new Vector3(0.5f, 0.5f), new Vector3(0, 0.5f));
		horizontalRaycastPointOffsets[1] = AddRaycastOffsetPoint(new Vector3(0.5f, 0.5f), new Vector3(1, 0.5f));
		horizontalRaycastPointOffsets[2] = AddRaycastOffsetPoint(new Vector3(0.5f, 0), new Vector3(0, 0));
		horizontalRaycastPointOffsets[3] = AddRaycastOffsetPoint(new Vector3(0.5f, 0), new Vector3(1, 0));
		horizontalRaycastPointOffsets[4] = AddRaycastOffsetPoint(new Vector3(0.5f, 1), new Vector3(0, 1));
		horizontalRaycastPointOffsets[5] = AddRaycastOffsetPoint(new Vector3(0.5f, 1), new Vector3(1, 1));
//		horizontalRaycastPointOffsets[2] = AddRaycastOffsetPoint(new Vector3(0, 0));
//		horizontalRaycastPointOffsets[3] = AddRaycastOffsetPoint(new Vector3(1, 0));
//		horizontalRaycastPointOffsets[4] = AddRaycastOffsetPoint(new Vector3(0, 1));
//		horizontalRaycastPointOffsets[5] = AddRaycastOffsetPoint(new Vector3(1, 1));

		verticalRaycastPointOffsets[0] = AddRaycastOffsetPoint(new Vector3(0.5f, 0.5f), new Vector3(0.5f, 0));
		verticalRaycastPointOffsets[1] = AddRaycastOffsetPoint(new Vector3(0.5f, 0.5f), new Vector3(0.5f, 1));
		verticalRaycastPointOffsets[2] = AddRaycastOffsetPoint(new Vector3(0, 0.5f), new Vector3(0, 0));
		verticalRaycastPointOffsets[3] = AddRaycastOffsetPoint(new Vector3(0, 0.5f), new Vector3(0, 1));
		verticalRaycastPointOffsets[4] = AddRaycastOffsetPoint(new Vector3(1, 0.5f), new Vector3(1, 0));
		verticalRaycastPointOffsets[5] = AddRaycastOffsetPoint(new Vector3(1, 0.5f), new Vector3(1, 1));

//		verticalRaycastPointOffsets[2] = AddRaycastOffsetPoint(new Vector3(0, 0));
//		verticalRaycastPointOffsets[3] = AddRaycastOffsetPoint(new Vector3(1, 0));
//		verticalRaycastPointOffsets[4] = AddRaycastOffsetPoint(new Vector3(0, 1));
//		verticalRaycastPointOffsets[5] = AddRaycastOffsetPoint(new Vector3(1, 1));

	}

	float CalculateDistanceReduction(OffsetData[] offsets, Vector3 idealCenterPoint) {
		RaycastHit hitInfo;
		var distanceReductionDueToCollision = 0f;

		offsets.Each(offset => {
			if(Physics.Raycast(idealCenterPoint + offset.StartPointRelativeToCamera, offset.NormalizedVector, out hitInfo, offset.DistanceFromStartPoint, cameraBumperLayers)) {
				var collisionDistance = offset.DistanceFromStartPoint - hitInfo.distance;
				if(collisionDistance > distanceReductionDueToCollision) distanceReductionDueToCollision = collisionDistance;
				if(drawDebugLines) Debug.DrawLine(idealCenterPoint + offset.StartPointRelativeToCamera, idealCenterPoint + offset.StartPointRelativeToCamera + (offset.NormalizedVector * hitInfo.distance), Color.red);
			}
			else if(drawDebugLines) Debug.DrawLine(idealCenterPoint + offset.StartPointRelativeToCamera, idealCenterPoint + offset.StartPointRelativeToCamera + offset.Vector, Color.green);
		});
		return distanceReductionDueToCollision;
	}
}