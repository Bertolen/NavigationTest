using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraController : MonoBehaviour {

	public Transform target;
	public Vector3 offset;
	public float pitch = 2f;
	public float zoomSpeed = 4f;
	public float minZoom = 5f;
	public float maxZoom = 15f;
	public float pitchSpeed = 0.01f;
	public float minOffsetY = -0.1f;

	private float currentZoom = 10f;

	void Update()
	{
		currentZoom -= Input.GetAxis ("Mouse ScrollWheel") * zoomSpeed;
		currentZoom = Mathf.Clamp (currentZoom, minZoom, maxZoom);

		UpdateCameraHeight ();
	}

	void UpdateCameraHeight()
	{
		offset.y -= pitchSpeed * Input.GetAxis ("Mouse Y");

		if(offset.y < minOffsetY)
		{
			offset.y = minOffsetY;
		}
	}

	void LateUpdate ()
	{
		transform.position = target.TransformPoint (offset * currentZoom);
		transform.LookAt (target.position + Vector3.up * pitch);
	}
}
