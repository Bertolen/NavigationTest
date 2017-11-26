using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;

public class Rope : Usable {

	public float climbSpeed = 1f;
	public float grabbingTime = 0.5f;
	[Range(0.1f, 1f)]
	public float distance = 0.5f;
	[Range(-180f, 180f)]
	public float lookingAngle = 15f;

	InputManager input;
	PlayerController playerController;
	Transform player;
	Rigidbody rb;
	bool isClimbing = false;
	bool isGettingInPosition = false;
	float maxHeight;
	float minHeight;
	float timer;

	void Start()
	{
		input = InputManager.instance;
		maxHeight = this.transform.position.y + this.transform.localScale.y;
		minHeight = this.transform.position.y - this.transform.localScale.y;
	}

	public override void Use(GameObject user)
	{
		player = user.transform;
		rb = user.GetComponent<Rigidbody> ();
		playerController = user.GetComponent<PlayerController> ();
		rb.useGravity = false;
		rb.angularVelocity = Vector3.zero;
		rb.velocity = Vector3.zero;
		playerController.enabled = false;
		isClimbing = true;
		isGettingInPosition = true;
		timer = 0;
	}

	void Release()
	{
		isClimbing = false;
		isGettingInPosition = false;
		rb.useGravity = true;
		playerController.enabled = true;
		playerController.ResetYaw ();

		if(input.GetAxis ("Horizontal") != 0f || input.GetAxis ("Vertical") != 0f) {
			playerController.movementSettings.acceleration *= 2;
			playerController.Move ();
			playerController.movementSettings.acceleration /= 2;
		}
	}

	void Update()
	{
		if(isClimbing && input.GetButtonDown ("Jump")){
			Release ();
		}

		if (isGettingInPosition) {
			GetInPosition ();
		} else if(isClimbing){
			Climb ();
			UpdateRotation ();
		}
	}

	void GetInPosition ()
	{
		float currentDistance = Vector3.Cross (this.transform.up, player.position - this.transform.position).magnitude;
		Vector3 positionDirection = Vector3.ProjectOnPlane (this.transform.position - player.position, this.transform.up).normalized;

		float currentAngle;
		Vector3 axis;
		Quaternion currentRotationDelta = Quaternion.FromToRotation (positionDirection, player.forward);
		currentRotationDelta.ToAngleAxis (out currentAngle, out axis);

		if( Mathf.Abs (currentAngle - lookingAngle) <= 0.001f && Mathf.Abs (currentDistance - distance) <= 0.001f){
			isGettingInPosition = false;
			return;
		}

		timer += Time.deltaTime;

		Vector3 desiredPosition = player.position + positionDirection * (currentDistance - distance);
		player.position = Vector3.Lerp (player.position, desiredPosition, timer / grabbingTime);

		Quaternion desiredRotation = Quaternion.AngleAxis (lookingAngle, this.transform.up) * Quaternion.FromToRotation (Vector3.forward, positionDirection);
		player.rotation = Quaternion.Lerp (player.rotation, desiredRotation, timer / grabbingTime);
	}

	void Climb()
	{
		float movement = input.GetAxis ("Vertical");
		float height = player.transform.position.y + movement * climbSpeed * Time.deltaTime;
		height = Mathf.Clamp (height, minHeight, maxHeight);

		Vector3 newPosition = player.transform.position;
		newPosition.y = height;
		player.transform.position = newPosition;
	}

	void UpdateRotation()
	{
		float yaw = playerController.yawSpeed * Input.GetAxis ("Mouse X") * Time.deltaTime;
		player.transform.RotateAround (this.transform.position, this.transform.up, yaw);
	}
}
