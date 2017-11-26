using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawPatrolPath : MonoBehaviour {

	void OnDrawGizmos()
	{
		Vector3 startPosition = this.transform.GetChild (0).position;
		Vector3 previousPosition = startPosition;

		foreach (Transform waypoint in this.transform)
		{
			Gizmos.DrawSphere (waypoint.position, 0.3f);
			Gizmos.DrawLine (previousPosition,waypoint.position);

			previousPosition = waypoint.position;
		}
		Gizmos.DrawLine (previousPosition,startPosition);
	}
}
