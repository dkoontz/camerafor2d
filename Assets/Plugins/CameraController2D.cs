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
	
	OffsetData leftRaycastPoint;
	OffsetData upperLeftRaycastPoint;
	OffsetData lowerLeftRaycastPoint;
	OffsetData rightRaycastPoint;
	OffsetData upperRightRaycastPoint;
	OffsetData lowerRightRaycastPoint;

	OffsetData upRaycastPoint;
	OffsetData downRaycastPoint;
	OffsetData leftUpRaycastPoint;
	OffsetData rightUpRaycastPoint;
	OffsetData leftDownRaycastPoint;
	OffsetData rightDownRaycastPoint;
	Stack<IEnumerable<Transform>> targetStack = new Stack<IEnumerable<Transform>>();

	public void AddTarget(Transform target) {
		AddTarget(new [] { target });
	}

	public void AddTarget(IEnumerable<Transform> targets) {
		targetStack.Push(targets);
	}

	public void Start() {
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
	
	public void LateUpdate() {
		var idealPosition = IdealCameraPosition();
		var vectorToIdealPosition = (idealPosition - transform.position);
		var normalizedVectorToIdealPosition = vectorToIdealPosition.normalized;
		var distanceToIdealPosition = vectorToIdealPosition.magnitude;

//		if(distanceToIdealPosition > 0) {
//			var shortestDistance = distanceToIdealPosition;
			var idealCenterPointAtPlayerHeight = idealPosition + HeightOffset();
//			var horizontalDistanceReductionDueToCollision = 0;
//			var verticalDistanceReductionDueToCollision = 0;
//
//			// check left horizontal and if not blocked, left side vertical
//			var leftCheck = CalculateDistanceReduction(leftRaycastPoint, idealCenterPoint);
//			if(leftCheck > 0) {
//				horizontalDistanceReductionDueToCollision = leftCheck;
//
//			}
//
//		}
			var horizontalVector = GetHorizontalComponent(Vector3.one).normalized;
			var verticalVector = GetVerticalComponent(Vector3.one).normalized;

			Debug.DrawLine(transform.position, idealPosition, Color.green);
			var horizontalDotProduct = Vector3.Dot(horizontalVector, normalizedVectorToIdealPosition);
			var horizontalFacing = 0;
			if(horizontalDotProduct > 0) horizontalFacing = 1;
			else if(horizontalDotProduct < 0) horizontalFacing = -1;
			
			var verticalFacing = 0;
			var verticalDotProduct = Vector3.Dot(verticalVector, normalizedVectorToIdealPosition);
			if(verticalDotProduct > 0) verticalFacing = 1;
			else if(verticalDotProduct < 0) verticalFacing = -1;

			var horizontalPushBack = 0f;
			var verticalPushBack = 0f;

//			Debug.Log("Horizontal: " + horizontalFacing + ", vertical: " + verticalFacing);

			
			horizontalPushBack = Mathf.Max(horizontalPushBack, CalculatePushback(rightRaycastPoint, idealCenterPointAtPlayerHeight));
			if(0 == horizontalPushBack) {
				if(1 == verticalFacing) {
					verticalPushBack = Mathf.Max(verticalPushBack, CalculatePushback(rightUpRaycastPoint, idealCenterPointAtPlayerHeight));
				}
				else if(-1 == verticalFacing) {
					verticalPushBack = Mathf.Max(verticalPushBack, CalculatePushback(rightDownRaycastPoint, idealCenterPointAtPlayerHeight));
				}
			}
		    horizontalPushBack = Mathf.Max(horizontalPushBack, CalculatePushback(leftRaycastPoint, idealCenterPointAtPlayerHeight));
			if(0 == horizontalPushBack) {
				if(1 == verticalFacing) {
					verticalPushBack = Mathf.Max(verticalPushBack, CalculatePushback(leftUpRaycastPoint, idealCenterPointAtPlayerHeight));
				}
				else if(-1 == verticalFacing) {
					verticalPushBack = Mathf.Max(verticalPushBack, CalculatePushback(leftDownRaycastPoint, idealCenterPointAtPlayerHeight));
				}
			}
		
            verticalPushBack = Mathf.Max(verticalPushBack, CalculatePushback(upRaycastPoint, idealCenterPointAtPlayerHeight));
			if(0 == verticalPushBack) {
				if(1 == horizontalFacing) {
					horizontalPushBack = Mathf.Max(horizontalPushBack, CalculatePushback(upperRightRaycastPoint, idealCenterPointAtPlayerHeight));
				}
				else if(-1 == horizontalFacing) {
					horizontalPushBack = Mathf.Max(horizontalPushBack, CalculatePushback(upperLeftRaycastPoint, idealCenterPointAtPlayerHeight));
				}
			}
			verticalPushBack = Mathf.Max(verticalPushBack, CalculatePushback(downRaycastPoint, idealCenterPointAtPlayerHeight));
			if(0 == verticalPushBack) {
				if(1 == horizontalFacing) {
					horizontalPushBack = Mathf.Max(horizontalPushBack, CalculatePushback(lowerRightRaycastPoint, idealCenterPointAtPlayerHeight));
				}
				else if(-1 == horizontalFacing) {
					horizontalPushBack = Mathf.Max(horizontalPushBack, CalculatePushback(lowerLeftRaycastPoint, idealCenterPointAtPlayerHeight));
				}
			}
		
//			var horizontalVector = GetHorizontalComponent(vectorToIdealPosition);
//			var idealHorizontalDistance = horizontalVector.magnitude;
//			horizontalVector = horizontalVector.normalized * Mathf.Max(idealHorizontalDistance - horizontalPushBack, 0);
//			//			Debug.Log("h: " + distanceReductionDueToCollision);
//			
//			var vertiallDistanceReductionDueToCollision = CalculateDistanceReduction(verticalRaycastPointOffsets, idealCenterPoint);
//			var verticalVector = GetVerticalComponent(vectorToIdealPosition);
//			var idealVerticalDistance = verticalVector.magnitude;
//			verticalVector = verticalVector.normalized * Mathf.Max(idealVerticalDistance - vertiallDistanceReductionDueToCollision, 0);
//			//			Debug.Log("v: " + distanceReductionDueToCollision);
//			
//			if(drawDebugLines) Debug.DrawLine(transform.position, idealPosition, Color.blue);
//			transform.Translate(horizontalVector + verticalVector, Space.World);
//			Debug.Log("horizontal: " + (horizontalVector * -horizontalPushBack) + ", vertical: " + (verticalVector * -verticalPushBack));
			transform.position = IdealCameraPosition() + (verticalVector * -verticalPushBack * verticalFacing) + (horizontalVector * -horizontalPushBack * horizontalFacing);
//		}
	}

	void CalculateScreenBounds() {
		System.Func<Vector3, Vector3, OffsetData> AddRaycastOffsetPoint = (viewSpaceOrigin, viewSpacePoint) => {
			var origin = camera.ViewportToWorldPoint(viewSpaceOrigin);
			var vectorToOffset = camera.ViewportToWorldPoint(viewSpacePoint) - origin;
//			Debug.Log("origin: " + origin + ", vectorToOffset: " + vectorToOffset);
			return new OffsetData { StartPointRelativeToCamera = origin - transform.position, Vector = vectorToOffset, NormalizedVector = vectorToOffset.normalized, DistanceFromStartPoint = vectorToOffset.magnitude };
		};

		leftRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 0.5f), new Vector3(0, 0.5f));
		rightRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 0.5f), new Vector3(1, 0.5f));
		lowerLeftRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 0), new Vector3(0, 0));
		lowerRightRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 0), new Vector3(1, 0));
		upperLeftRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 1), new Vector3(0, 1));
		upperRightRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 1), new Vector3(1, 1));
//		horizontalRaycastPointOffsets[2] = AddRaycastOffsetPoint(new Vector3(0, 0));
//		horizontalRaycastPointOffsets[3] = AddRaycastOffsetPoint(new Vector3(1, 0));
//		horizontalRaycastPointOffsets[4] = AddRaycastOffsetPoint(new Vector3(0, 1));
//		horizontalRaycastPointOffsets[5] = AddRaycastOffsetPoint(new Vector3(1, 1));

		downRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 0.5f), new Vector3(0.5f, 0));
		upRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 0.5f), new Vector3(0.5f, 1));
		leftDownRaycastPoint = AddRaycastOffsetPoint(new Vector3(0, 0.5f), new Vector3(0, 0));
		leftUpRaycastPoint = AddRaycastOffsetPoint(new Vector3(0, 0.5f), new Vector3(0, 1));
		rightDownRaycastPoint = AddRaycastOffsetPoint(new Vector3(1, 0.5f), new Vector3(1, 0));
		rightUpRaycastPoint = AddRaycastOffsetPoint(new Vector3(1, 0.5f), new Vector3(1, 1));

//		verticalRaycastPointOffsets[2] = AddRaycastOffsetPoint(new Vector3(0, 0));
//		verticalRaycastPointOffsets[3] = AddRaycastOffsetPoint(new Vector3(1, 0));
//		verticalRaycastPointOffsets[4] = AddRaycastOffsetPoint(new Vector3(0, 1));
//		verticalRaycastPointOffsets[5] = AddRaycastOffsetPoint(new Vector3(1, 1));

	}

	float CalculatePushback(OffsetData offset, Vector3 idealCenterPoint) {
		RaycastHit hitInfo;
		var pushbackDueToCollision = 0f;

		if(Physics.Raycast(idealCenterPoint + offset.StartPointRelativeToCamera, offset.NormalizedVector, out hitInfo, offset.DistanceFromStartPoint, cameraBumperLayers)) {
			if(drawDebugLines) Debug.DrawLine(idealCenterPoint + offset.StartPointRelativeToCamera, idealCenterPoint + offset.StartPointRelativeToCamera + (offset.NormalizedVector * hitInfo.distance), Color.red);
			pushbackDueToCollision = offset.DistanceFromStartPoint - hitInfo.distance;
		}
		else if(drawDebugLines) Debug.DrawLine(idealCenterPoint + offset.StartPointRelativeToCamera, idealCenterPoint + offset.StartPointRelativeToCamera + offset.Vector, Color.green);

		return pushbackDueToCollision;
	}
}