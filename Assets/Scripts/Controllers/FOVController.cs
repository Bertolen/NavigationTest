using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVController : MonoBehaviour {

	[Range (0f, 1f)]
	public float nearDst = 0.3f;
	[Range (1f, 100f)]
	public float farDst = 15f;
	[Range (0f, 180f)]
	public float verticalAngle = 40f;
	[Range (0f, 180f)]
	public float horizontalAngle = 85f;

	public Transform planes;

	[Header("Vertices")]
	public Transform near;
	public Transform far;
	public Transform v1;
	public Transform v2;
	public Transform v3;
	public Transform v4;
	public Transform v5;
	public Transform v6;
	public Transform v7;
	public Transform v8;

	void OnDrawGizmos()
	{
		PlaceVertices ();
		PlacePlanes ();

		DrawRectangle (v1.position,v2.position,v3.position,v4.position);
		DrawRectangle (v5.position,v6.position,v7.position,v8.position);

		Gizmos.DrawLine (v1.position,v5.position);
		Gizmos.DrawLine (v2.position,v6.position);
		Gizmos.DrawLine (v3.position,v7.position);
		Gizmos.DrawLine (v4.position,v8.position);

		Gizmos.DrawSphere (near.position,0.1f);
		Gizmos.DrawSphere (far.position,0.1f);
	}

	void DrawRectangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
	{
		Gizmos.DrawLine (v1,v2);
		Gizmos.DrawLine (v2,v3);
		Gizmos.DrawLine (v3,v4);
		Gizmos.DrawLine (v4,v1);
	}

	void Update()
	{
		PlaceVertices ();
		PlacePlanes ();
	}

	void PlaceVertices()
	{
		float hAngle = Mathf.Deg2Rad * horizontalAngle / 2;
		float vAngle = Mathf.Deg2Rad * verticalAngle / 2;

		near.position = this.transform.forward * nearDst + this.transform.position;
		far.position = this.transform.forward * farDst + this.transform.position;
		v1.position = near.position + this.transform.right * (Mathf.Tan (hAngle) * nearDst) + this.transform.up * (Mathf.Tan (vAngle) * nearDst);
		v2.position = near.position + this.transform.right * (Mathf.Tan (-hAngle) * nearDst) + this.transform.up * (Mathf.Tan (vAngle) * nearDst);
		v3.position = near.position + this.transform.right * (Mathf.Tan (-hAngle) * nearDst) + this.transform.up * (Mathf.Tan (-vAngle) * nearDst);
		v4.position = near.position + this.transform.right * (Mathf.Tan (hAngle) * nearDst) + this.transform.up * (Mathf.Tan (-vAngle) * nearDst);
		v5.position = far.position + this.transform.right * (Mathf.Tan (hAngle) * farDst) + this.transform.up * (Mathf.Tan (vAngle) * farDst);
		v6.position = far.position + this.transform.right * (Mathf.Tan (-hAngle) * farDst) + this.transform.up * (Mathf.Tan (vAngle) * farDst);
		v7.position = far.position + this.transform.right * (Mathf.Tan (-hAngle) * farDst) + this.transform.up * (Mathf.Tan (-vAngle) * farDst);
		v8.position = far.position + this.transform.right * (Mathf.Tan (hAngle) * farDst) + this.transform.up * (Mathf.Tan (-vAngle) * farDst);
	}

	public Plane[] CalculateFrustumPlanes()
	{
		Plane[] frustumPlanes = new Plane[6];
		frustumPlanes [0] = new Plane (v2.position, v6.position, v7.position);
		frustumPlanes [1] = new Plane (v5.position, v1.position, v4.position);
		frustumPlanes [2] = new Plane (v7.position, v8.position, v4.position);
		frustumPlanes [3] = new Plane (v5.position, v6.position, v2.position);
		frustumPlanes [4] = new Plane (near.position, v1.position, v2.position);
		frustumPlanes [5] = new Plane (far.position, v6.position, v5.position);

		return frustumPlanes;
	}

	void PlacePlanes()
	{
		Plane[] frustumPlanes = CalculateFrustumPlanes ();

		for (int i = 0 ; i < frustumPlanes.Length ; i++) {
			planes.transform.GetChild (i).position = -frustumPlanes [i].normal * frustumPlanes [i].distance;
			planes.transform.GetChild (i).rotation = Quaternion.FromToRotation (Vector3.up, frustumPlanes [i].normal);
		}
	}
}
