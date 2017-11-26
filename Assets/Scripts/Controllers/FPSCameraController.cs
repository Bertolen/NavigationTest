using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCameraController : MonoBehaviour {

	public float peekOffset = 0.2f;
	public float speed = 0.5f; 
	public float pitchSpeed = 1f;
	public float maxPitch = 90f;
	public float minPitch = -90f;
	public Transform characterTransform;

	InputManager input;
	Vector3 wantedPlacement;
	Vector3 placementOffset;
	float pitch;

	void Start()
	{
		input = InputManager.instance;
		placementOffset = characterTransform.InverseTransformPoint (this.transform.position);
		pitch = 0f;
	}

	void Update () 
	{
		UpdatePlacement ();

		UpdatePitch ();
	}

	void UpdatePlacement ()
	{
		Vector3 localPlacement = new Vector3(0,0,0);

		float peekCoef = input.GetAxis ("Peek");

		localPlacement.x = peekOffset * peekCoef;

		wantedPlacement = characterTransform.TransformPoint (localPlacement + placementOffset);

		this.transform.position = Vector3.MoveTowards (this.transform.position, wantedPlacement, speed * Time.deltaTime);
	}

	void UpdatePitch()
	{
		pitch -= pitchSpeed * Input.GetAxis ("Mouse Y") * Time.deltaTime;

		if (pitch < minPitch)
			pitch = minPitch;

		if (pitch > maxPitch)
			pitch = maxPitch;

		Vector3 vector = Vector3.right;

		this.transform.localRotation = Quaternion.Euler (pitch, 0f, 0f);
	}
}
