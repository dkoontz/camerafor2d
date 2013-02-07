using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoodStuff.NaturalLanguage;

public class CameraController2D : MonoBehaviour {
	public enum MovementAxis {
//		XY,
		XZ,
//		YZ
	}

	class OffsetData {
		public Vector3 StartPointRelativeToCamera { get; set; }
		public Vector3 Vector { get; set; }
		public Vector3 NormalizedVector { get; set; }
		public float DistanceFromStartPoint { get; set; }
	}
	
	public MovementAxis axis = MovementAxis.XZ;
	public LayerMask cameraBumperLayers;
	public float distance;
	public float maxMoveSpeedPerSecond = 1;
	public Transform initialTarget;

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
	bool panningToNewTarget;
	float panningToNewTargetSpeed;

	public void AddTarget(Transform target) {
		AddTarget(new [] { target });
	}

	public void AddTarget(Transform target, float moveSpeed) {
		AddTarget(new [] { target }, moveSpeed);
	}

	public void AddTarget(Transform target, float moveSpeed, float revertAfterDuration, bool snapBackWhenReverting) {
		AddTarget(new [] { target }, moveSpeed, revertAfterDuration, snapBackWhenReverting);
	}

	public void AddTarget(IEnumerable<Transform> targets) {
		targetStack.Push(targets);
		panningToNewTarget = true;
	}

	public void AddTarget(IEnumerable<Transform> targets, float moveSpeed) {
		targetStack.Push(targets);
		panningToNewTarget = true;
		panningToNewTargetSpeed = moveSpeed;
	}

	public void AddTarget(IEnumerable<Transform> targets, float moveSpeed, float revertAfterDuration, bool snapBackWhenReverting) {
		targetStack.Push(targets);
		panningToNewTarget = true;
		panningToNewTargetSpeed = moveSpeed;
		StartCoroutine(RemoveTargetAfterDelay(revertAfterDuration, snapBackWhenReverting));
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
		AddTarget(initialTarget);

		CalculateScreenBounds();
		IdealCameraPosition = () => {
			if(1 == targetStack.Peek ().Count()) return targetStack.Peek().First().position - HeightOffset();

			var minHorizontal = targetStack.Peek().Min(t => GetHorizontalValue(t.position));
			var maxHorizontal = targetStack.Peek().Max(t => GetHorizontalValue(t.position));
			var horizontalOffset = (maxHorizontal - minHorizontal) * 0.5f;
			var minVertical = targetStack.Peek().Min(t => GetVerticalValue(t.position));
			var maxVertical = targetStack.Peek().Max(t => GetVerticalValue(t.position));
			var verticalOffset = (maxVertical - minVertical) * 0.5f;
			return (GetHorizontalComponent(Vector3.one) * (minHorizontal + horizontalOffset)) + (GetVerticalComponent(Vector3.one) * (minVertical + verticalOffset)) - HeightOffset();
		};

		JumpToIdealPosition();
	}
	
	public void LateUpdate() {
		var idealPosition = IdealCameraPosition();
		var vectorToIdealPosition = (idealPosition - transform.position);
		var distanceToIdealPosition = vectorToIdealPosition.magnitude;

		if(distanceToIdealPosition == 0) {
			panningToNewTarget = false;
		}
		else {
			var idealCenterPointAtPlayerHeight = idealPosition + HeightOffset();
			var horizontalVector = GetHorizontalComponent(Vector3.one).normalized;
			var verticalVector = GetVerticalComponent(Vector3.one).normalized;

			var horizontalFacing = 0;
			var verticalFacing = 0;

			var horizontalPushBack = 0f;
			var verticalPushBack = 0f;
			var rightHorizontalPushBack = 0f;
			var leftHorizontalPushBack = 0f;
			var upVerticalPushBack = 0f;
			var downVerticalPushBack = 0f;

			rightHorizontalPushBack = CalculatePushback(rightRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.HorizontalPositive);
			if(rightHorizontalPushBack > horizontalPushBack) {
				horizontalPushBack = rightHorizontalPushBack;
				horizontalFacing = 1;
			}
			if(0 == rightHorizontalPushBack) {
				upVerticalPushBack = CalculatePushback(rightUpRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.VerticalPositive);
				if(upVerticalPushBack > verticalPushBack) {
					verticalPushBack = upVerticalPushBack;
					verticalFacing = 1;
				}
				downVerticalPushBack = CalculatePushback(rightDownRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.VerticalNegative);
				if(downVerticalPushBack > verticalPushBack) {
					verticalPushBack = downVerticalPushBack;
					verticalFacing = -1;
				}
			}

			leftHorizontalPushBack = CalculatePushback(leftRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.HorizontalNegative);
			if(leftHorizontalPushBack > horizontalPushBack) {
				horizontalPushBack = leftHorizontalPushBack;
				horizontalFacing = -1;
			}
			if(0 == leftHorizontalPushBack) {
				upVerticalPushBack = CalculatePushback(leftUpRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.VerticalPositive);
				if(upVerticalPushBack > verticalPushBack) {
					verticalPushBack = upVerticalPushBack;
					verticalFacing = 1;
				}
				downVerticalPushBack = CalculatePushback(leftDownRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.VerticalNegative);
				if(downVerticalPushBack > verticalPushBack) {
					verticalPushBack = downVerticalPushBack;
					verticalFacing = -1;
				}
			}
		
			upVerticalPushBack = CalculatePushback(upRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.VerticalPositive);
			if(upVerticalPushBack > verticalPushBack) {
				verticalPushBack = upVerticalPushBack;
				verticalFacing = 1;
			}
			if(0 == upVerticalPushBack) {
				rightHorizontalPushBack = CalculatePushback(upperRightRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.HorizontalPositive);
				if(rightHorizontalPushBack > horizontalPushBack) {
					horizontalPushBack = rightHorizontalPushBack;
					horizontalFacing = 1;
				}
				leftHorizontalPushBack = CalculatePushback(upperLeftRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.HorizontalNegative);
				if(leftHorizontalPushBack > horizontalPushBack) {
					horizontalPushBack = leftHorizontalPushBack;
					horizontalFacing = -1;
				}
			}

			downVerticalPushBack = CalculatePushback(downRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.VerticalNegative);
			if(downVerticalPushBack > verticalPushBack)  {
				verticalPushBack = downVerticalPushBack;
				verticalFacing = -1;
			}
			if(0 == downVerticalPushBack) {
				rightHorizontalPushBack = CalculatePushback(lowerRightRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.HorizontalPositive);
				if(rightHorizontalPushBack > horizontalPushBack) {
					horizontalPushBack = rightHorizontalPushBack;
					horizontalFacing = 1;
				}
				leftHorizontalPushBack = CalculatePushback(lowerLeftRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.HorizontalNegative);
				if(leftHorizontalPushBack > horizontalPushBack) {
					horizontalPushBack = leftHorizontalPushBack;
					horizontalFacing = -1;
				}
			}
		
//			Debug.DrawLine(transform.position, idealPosition, Color.blue);
//			Debug.Log("vertical pushback: " + verticalPushBack);
			var targetVector = (idealPosition + (verticalVector * -verticalPushBack * verticalFacing) + (horizontalVector * -horizontalPushBack * horizontalFacing)) - transform.position;
			var moveSpeed = maxMoveSpeedPerSecond;
			if(panningToNewTarget) moveSpeed = panningToNewTargetSpeed;
			transform.Translate(targetVector.normalized * Mathf.Min(targetVector.magnitude, moveSpeed * Time.deltaTime), Space.World);
		}
	}

	public void JumpToIdealPosition() {
		transform.position = IdealCameraPosition();
	}

	void CalculateScreenBounds() {
		System.Func<Vector3, Vector3, OffsetData> AddRaycastOffsetPoint = (viewSpaceOrigin, viewSpacePoint) => {
			var origin = camera.ViewportToWorldPoint(viewSpaceOrigin);
			var vectorToOffset = camera.ViewportToWorldPoint(viewSpacePoint) - origin;
			return new OffsetData { StartPointRelativeToCamera = origin - transform.position, Vector = vectorToOffset, NormalizedVector = vectorToOffset.normalized, DistanceFromStartPoint = vectorToOffset.magnitude };
		};

		leftRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 0.5f), new Vector3(0, 0.5f));
		rightRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 0.5f), new Vector3(1, 0.5f));
		lowerLeftRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 0), new Vector3(0, 0));
		lowerRightRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 0), new Vector3(1, 0));
		upperLeftRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 1), new Vector3(0, 1));
		upperRightRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 1), new Vector3(1, 1));

		downRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 0.5f), new Vector3(0.5f, 0));
		upRaycastPoint = AddRaycastOffsetPoint(new Vector3(0.5f, 0.5f), new Vector3(0.5f, 1));
		leftUpRaycastPoint = AddRaycastOffsetPoint(new Vector3(0, 0.5f), new Vector3(0, 1));
		leftDownRaycastPoint = AddRaycastOffsetPoint(new Vector3(0, 0.5f), new Vector3(0, 0));
		rightUpRaycastPoint = AddRaycastOffsetPoint(new Vector3(1, 0.5f), new Vector3(1, 1));
		rightDownRaycastPoint = AddRaycastOffsetPoint(new Vector3(1, 0.5f), new Vector3(1, 0));
	}

	float CalculatePushback(OffsetData offset, Vector3 idealCenterPoint, CameraBumper.BumperDirection validDirections = CameraBumper.BumperDirection.AllDirections) {
		RaycastHit hitInfo;
		var pushbackDueToCollision = 0f;

		if(Physics.Raycast(idealCenterPoint + offset.StartPointRelativeToCamera, offset.NormalizedVector, out hitInfo, offset.DistanceFromStartPoint, cameraBumperLayers)) {
			var bumper = hitInfo.collider.GetComponent<CameraBumper>();
			if(null == bumper || (bumper != null && (bumper.blockDirection & validDirections) != CameraBumper.BumperDirection.None)) {
				pushbackDueToCollision = offset.DistanceFromStartPoint - hitInfo.distance;
				if(drawDebugLines) Debug.DrawLine(idealCenterPoint + offset.StartPointRelativeToCamera, idealCenterPoint + offset.StartPointRelativeToCamera + (offset.NormalizedVector * hitInfo.distance), Color.red);
			}
		}
		else if(drawDebugLines) Debug.DrawLine(idealCenterPoint + offset.StartPointRelativeToCamera, idealCenterPoint + offset.StartPointRelativeToCamera + offset.Vector, Color.green);

		return pushbackDueToCollision;
	}

	IEnumerator RemoveTargetAfterDelay(float delay, bool snapBack) {
		yield return new WaitForSeconds(delay);
		panningToNewTarget = true;
		if(snapBack) panningToNewTargetSpeed = float.MaxValue;
		targetStack.Pop();
	}
}