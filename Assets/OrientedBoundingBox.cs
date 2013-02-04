using UnityEngine;
using System.Collections;

public class OrientedBoundingBox : MonoBehaviour {

	public Vector3 extents;
	public OrientedBoundingBox target;

	Vector3[] offsetPoints = new Vector3[8];

	public void Start() {
		offsetPoints[0] = new Vector3(-extents.x, extents.y, -extents.z);
		offsetPoints[1] = new Vector3(extents.x, extents.y, -extents.z);
		offsetPoints[2] = new Vector3(extents.x, -extents.y, -extents.z);
		offsetPoints[3] = new Vector3(-extents.x, -extents.y, -extents.z);
		offsetPoints[4] = new Vector3(extents.x, extents.y, extents.z);
		offsetPoints[5] = new Vector3(extents.x, -extents.y, extents.z);
		offsetPoints[6] = new Vector3(-extents.x, extents.y, extents.z);
		offsetPoints[7] = new Vector3(-extents.x, -extents.y, extents.z);
	}

	public bool CollidesWith(OrientedBoundingBox other) {
		var otherPoints = other.OffsetPointsInWorldSpace();
		// check other points in our local space
		for(var i = 0; i < otherPoints.Length; ++i) {
			var point = otherPoints[i] = transform.InverseTransformPoint(otherPoints[i]);
		}

		Gizmos.color = Color.blue;
		DrawDebugLines(offsetPoints);
		Gizmos.color = Color.red;
		DrawDebugLines(otherPoints);

		// check our points in the other's local space

//		Debug.Log("intersects: " + collider.bounds.Intersects(other.collider.bounds));
		return true;
	}

	public void OnDrawGizmos() {
		var transformedPoints = OffsetPointsInWorldSpace();
		DrawDebugLines(transformedPoints);

		if(target != null) CollidesWith(target);
	}

	public Vector3[] OffsetPointsInWorldSpace() {
		var transformedPoints = new Vector3[8];
		for(var i = 0; i < transformedPoints.Length; ++i) transformedPoints[i] = transform.TransformPoint(offsetPoints[i]);
		return transformedPoints;
	}

	public void DrawDebugLines(Vector3[] points) {
		Gizmos.DrawLine(points[0], points[1]);
		Gizmos.DrawLine(points[1], points[2]);
		Gizmos.DrawLine(points[2], points[3]);
		Gizmos.DrawLine(points[0], points[3]);
		Gizmos.DrawLine(points[1], points[4]);
		Gizmos.DrawLine(points[4], points[5]);
		Gizmos.DrawLine(points[5], points[2]);
		Gizmos.DrawLine(points[4], points[6]);
		Gizmos.DrawLine(points[6], points[7]);
		Gizmos.DrawLine(points[7], points[5]);
		Gizmos.DrawLine(points[0], points[6]);
		Gizmos.DrawLine(points[3], points[7]);
	}
}
