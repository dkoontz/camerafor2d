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

	public Vector3 CameraSeekPosition { 
		get { return CameraSeekTarget.position; }
		set { 
			if(!exclusiveModeEnabled) throw new System.InvalidOperationException("Cannot set an explicit camera seek target unless the camera is in exclusive mode");
			CameraSeekTarget.position = value;
		}
	}

	public MovementAxis axis = MovementAxis.XZ;
	public LayerMask cameraBumperLayers;
	public float distance;
	public float maxMoveSpeedPerSecond = 10;
	public Transform initialTarget;
	public float damping = .5f;

	public bool drawDebugLines;

	const float CAMERA_ARRIVAL_DISTANCE = .001f;
	const float CAMERA_ARRIVAL_DISTANCE_SQUARED = CAMERA_ARRIVAL_DISTANCE * CAMERA_ARRIVAL_DISTANCE;

	Transform CameraSeekTarget { get; set; }

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
	
	Vector3 velocity;
	bool exclusiveModeEnabled;
	Stack<IEnumerable<Transform>> targetStack = new Stack<IEnumerable<Transform>>();
	List<Vector3> influences = new List<Vector3>(5);
	bool panningToNewTarget;
	float panningToNewTargetSpeed;
	Vector3 currentMovementVector;

	public void AddTarget(Transform target) {
		AddTarget(new [] { target });
	}

	public void AddTarget(Transform target, float moveSpeed) {
		AddTarget(new [] { target }, moveSpeed);
	}

	public void AddTarget(Transform target, float moveSpeed, float revertAfterDuration, float revertMoveSpeed) {
		AddTarget(new [] { target }, moveSpeed, revertAfterDuration, revertMoveSpeed);
	}

	public void AddTarget(IEnumerable<Transform> targets) {
		targetStack.Push(targets);
		panningToNewTarget = true;
		panningToNewTargetSpeed = maxMoveSpeedPerSecond;
	}

	public void AddTarget(IEnumerable<Transform> targets, float moveSpeed) {
		targetStack.Push(targets);
		panningToNewTarget = true;
		panningToNewTargetSpeed = moveSpeed;
	}

	public void AddTarget(IEnumerable<Transform> targets, float moveSpeed, float revertAfterDuration, float revertMoveSpeed) {
		targetStack.Push(targets);
		panningToNewTarget = true;
		panningToNewTargetSpeed = moveSpeed;
		StartCoroutine(RemoveTargetAfterDelay(revertAfterDuration, revertMoveSpeed));
	}

	public void RemoveCurrentTarget() {
		targetStack.Pop();
		panningToNewTarget = true;
		panningToNewTargetSpeed = maxMoveSpeedPerSecond;
	}

	public void JumpToIdealPosition() {
		if(!exclusiveModeEnabled) throw new System.InvalidOperationException("Cannot set an explicit camera position unless the camera is in exclusive mode");
		transform.position = IdealCameraPosition();
	}

	public void AddInfluence(Vector3 influence) {
		influences.Add(influence);
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

		CameraSeekTarget = new GameObject("_CameraTarget").transform;
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

//		softArrivalMinimumMovement = softArrivalDistance * .20f;
		exclusiveModeEnabled = true;
		JumpToIdealPosition();
		exclusiveModeEnabled = false;
	}
	
	public void LateUpdate() {
		if(Input.GetKeyDown(KeyCode.Alpha1)) {
			targetStack.Peek().First().Translate(-Vector3.right * 2);
		}
		if(Input.GetKeyDown(KeyCode.Alpha2)) {
			targetStack.Peek().First().Translate(Vector3.right * 2);
		}
		if(Input.GetKey(KeyCode.Alpha3)) {
			targetStack.Peek().First().Translate(-Vector3.right * 50 * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.Alpha4)) {
			targetStack.Peek().First().Translate(Vector3.right * 50 * Time.deltaTime);
		}

		if(!exclusiveModeEnabled) CameraSeekTarget.position = IdealCameraPosition() + TotalInfluence();

		var idealPosition = CameraSeekPosition;
		var vectorToIdealPosition = (idealPosition - transform.position);
		var distanceToIdealPosition = vectorToIdealPosition.magnitude;

		if(panningToNewTarget && distanceToIdealPosition < CAMERA_ARRIVAL_DISTANCE) {
			Debug.Log("arrived at target, need to add callback");
			panningToNewTarget = false;
		}
		else {
			var targetPosition = idealPosition + CalculatePushBackOffset(idealPosition);
			var targetVector = targetPosition - transform.position;
			var maxSpeed = maxMoveSpeedPerSecond;
			if(panningToNewTarget) maxSpeed = panningToNewTargetSpeed;

			var targetMagnitude = targetVector.magnitude;

			var interpolatedPosition = Vector3.zero;
			interpolatedPosition.x = Mathf.SmoothDamp(transform.position.x, targetPosition.x, ref velocity.x, damping, maxSpeed);
			interpolatedPosition.y = Mathf.SmoothDamp(transform.position.y, targetPosition.y, ref velocity.y, damping, maxSpeed);
			interpolatedPosition.z = Mathf.SmoothDamp(transform.position.z, targetPosition.z, ref velocity.z, damping, maxSpeed);

			transform.position = interpolatedPosition;

			influences.Clear();
		}
	}

	void CalculateScreenBounds() {
		System.Func<Vector3, Vector3, OffsetData> AddRaycastOffsetPoint = (viewSpaceOrigin, viewSpacePoint) => {
			if(camera.isOrthoGraphic) {
				var origin = camera.ViewportToWorldPoint(viewSpaceOrigin);
				var vectorToOffset = camera.ViewportToWorldPoint(viewSpacePoint) - origin;
				return new OffsetData { StartPointRelativeToCamera = origin - transform.position, Vector = vectorToOffset, NormalizedVector = vectorToOffset.normalized, DistanceFromStartPoint = vectorToOffset.magnitude };
			}
			else {
				var cameraPositionOnPlane = transform.position + (transform.forward * distance);

				var originRay = camera.ViewportPointToRay(viewSpaceOrigin);
				var theta = Vector3.Angle(transform.forward, originRay.direction);
				var distanceToPlane = distance / Mathf.Cos(theta * Mathf.Deg2Rad);
				var originPointOnPlane = originRay.origin + (originRay.direction * distanceToPlane);

				var pointRay = camera.ViewportPointToRay(viewSpacePoint);
				theta = Vector3.Angle(camera.transform.forward, pointRay.direction);
				distanceToPlane = distance / Mathf.Cos(theta * Mathf.Deg2Rad);
				var pointOnPlane = pointRay.origin + (pointRay.direction * distanceToPlane);
				var vectorToOffset = pointOnPlane - originPointOnPlane;

				return new OffsetData { StartPointRelativeToCamera = originPointOnPlane - cameraPositionOnPlane, Vector = vectorToOffset, NormalizedVector = vectorToOffset.normalized, DistanceFromStartPoint = vectorToOffset.magnitude };
			}
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

	Vector3 CalculatePushBackOffset(Vector3 idealPosition) {
		var idealCenterPointAtPlayerHeight = idealPosition + HeightOffset ();
		var horizontalVector = GetHorizontalComponent (Vector3.one).normalized;
		var verticalVector = GetVerticalComponent (Vector3.one).normalized;
		var horizontalFacing = 0;
		var verticalFacing = 0;
		var horizontalPushBack = 0f;
		var verticalPushBack = 0f;
		var rightHorizontalPushBack = 0f;
		var leftHorizontalPushBack = 0f;
		var upVerticalPushBack = 0f;
		var downVerticalPushBack = 0f;

		rightHorizontalPushBack = CalculatePushback (rightRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.HorizontalPositive);
		if (rightHorizontalPushBack > horizontalPushBack) {
			horizontalPushBack = rightHorizontalPushBack;
			horizontalFacing = 1;
		}
		if (0 == rightHorizontalPushBack) {
			upVerticalPushBack = CalculatePushback (rightUpRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.VerticalPositive);
			if (upVerticalPushBack > verticalPushBack) {
				verticalPushBack = upVerticalPushBack;
				verticalFacing = 1;
			}
			downVerticalPushBack = CalculatePushback (rightDownRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.VerticalNegative);
			if (downVerticalPushBack > verticalPushBack) {
				verticalPushBack = downVerticalPushBack;
				verticalFacing = -1;
			}
		}
		leftHorizontalPushBack = CalculatePushback (leftRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.HorizontalNegative);
		if (leftHorizontalPushBack > horizontalPushBack) {
			horizontalPushBack = leftHorizontalPushBack;
			horizontalFacing = -1;
		}
		if (0 == leftHorizontalPushBack) {
			upVerticalPushBack = CalculatePushback (leftUpRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.VerticalPositive);
			if (upVerticalPushBack > verticalPushBack) {
				verticalPushBack = upVerticalPushBack;
				verticalFacing = 1;
			}
			downVerticalPushBack = CalculatePushback (leftDownRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.VerticalNegative);
			if (downVerticalPushBack > verticalPushBack) {
				verticalPushBack = downVerticalPushBack;
				verticalFacing = -1;
			}
		}
		upVerticalPushBack = CalculatePushback (upRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.VerticalPositive);
		if (upVerticalPushBack > verticalPushBack) {
			verticalPushBack = upVerticalPushBack;
			verticalFacing = 1;
		}
		if (0 == upVerticalPushBack) {
			rightHorizontalPushBack = CalculatePushback (upperRightRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.HorizontalPositive);
			if (rightHorizontalPushBack > horizontalPushBack) {
				horizontalPushBack = rightHorizontalPushBack;
				horizontalFacing = 1;
			}
			leftHorizontalPushBack = CalculatePushback (upperLeftRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.HorizontalNegative);
			if (leftHorizontalPushBack > horizontalPushBack) {
				horizontalPushBack = leftHorizontalPushBack;
				horizontalFacing = -1;
			}
		}
		downVerticalPushBack = CalculatePushback (downRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.VerticalNegative);
		if (downVerticalPushBack > verticalPushBack) {
			verticalPushBack = downVerticalPushBack;
			verticalFacing = -1;
		}
		if (0 == downVerticalPushBack) {
			rightHorizontalPushBack = CalculatePushback (lowerRightRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.HorizontalPositive);
			if (rightHorizontalPushBack > horizontalPushBack) {
				horizontalPushBack = rightHorizontalPushBack;
				horizontalFacing = 1;
			}
			leftHorizontalPushBack = CalculatePushback (lowerLeftRaycastPoint, idealCenterPointAtPlayerHeight, CameraBumper.BumperDirection.HorizontalNegative);
			if (leftHorizontalPushBack > horizontalPushBack) {
				horizontalPushBack = leftHorizontalPushBack;
				horizontalFacing = -1;
			}
		}
		return (verticalVector * -verticalPushBack * verticalFacing) + (horizontalVector * -horizontalPushBack * horizontalFacing);
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

	IEnumerator RemoveTargetAfterDelay(float delay, float revertMoveSpeed) {
		yield return new WaitForSeconds(delay);
		panningToNewTarget = true;
		panningToNewTargetSpeed = revertMoveSpeed;
		targetStack.Pop();
	}

	Vector3 TotalInfluence() {
//		Debug.Log(influences.Aggregate(Vector3.zero, (offset, influence) => offset + influence));
		return influences.Aggregate(Vector3.zero, (offset, influence) => offset + influence);
	}

	void OnDrawGizmos() {
		if(Application.isPlaying && drawDebugLines) {
			var idealPosition = IdealCameraPosition();

			if(!exclusiveModeEnabled) {
				Gizmos.color = Color.magenta;
				influences.Each(influence => Gizmos.DrawLine(idealPosition, idealPosition + influence));
			}

			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(idealPosition + TotalInfluence(), .1f);

			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, transform.position + currentMovementVector);
		}
	}
}