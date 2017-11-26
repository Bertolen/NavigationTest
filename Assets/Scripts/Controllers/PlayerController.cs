using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(FPSCameraController))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour {

	[Serializable]
	public class MovementSettings {
		public float acceleration;
		public float speed;
		public float crouchingMultiplier;
		public float runningMultiplier;
		public float jumpForce;
		public float jumpOverHeight;
	}

	[Serializable]
	public struct HeightSettings{
		public float crouchSpeed;
		public float minimumHeight;
		public float maximumHeight;	
	}

	public float yaw = 0f;
	public float yawSpeed = 5f;
	public float soundRadius = 15f;
	public LayerMask enemyMask;

	public HeightSettings heightSettings;
	public MovementSettings movementSettings;

	CapsuleCollider playerColl;
	Vector3 direction;
	Rigidbody rb;
	InputManager input;
	LevelManager level;
	UIManager uiManager;
	FPSCameraController camController;
	AudioSource audioSource;
	float height;
	bool isCrouching;
	bool isRunning;
	bool isJumpingOver;
	float currentSpeed;
	List<Vector3> contactNormals;
	Vector3 landingPosition;
	bool highPointReached;
	Vector3 highPoint;
	bool canJumpOver;

	void Start () {
		audioSource = GetComponent<AudioSource> ();
		rb = GetComponent<Rigidbody> ();
		playerColl = GetComponent<CapsuleCollider> ();
		camController = GetComponentInChildren<FPSCameraController> ();
		input = InputManager.instance;
		level = LevelManager.instance;
		uiManager = UIManager.instance;
		ResetYaw ();
		isCrouching = false;
		isJumpingOver = false;
		canJumpOver = false;
		height = heightSettings.maximumHeight;
		level.HideCursor ();
		contactNormals = new List<Vector3> ();
		landingPosition = this.transform.position;
	}

	public void Activate()
	{
		this.enabled = true;
		camController.enabled = true;
	}

	public void Deactivate()
	{
		this.enabled = false;
		camController.enabled = false;
	}

	public void ResetYaw()
	{
		yaw = this.transform.eulerAngles.y;		
	}

	void Update()
	{
		if(isJumpingOver) {
			JumpOver ();
			return;
		}

		if (IsGrounded ()){
			
			Move ();

			if (!isCrouching && rb.velocity.magnitude >= movementSettings.speed){
				MakeNoise ();
			}
		} else {
			rb.drag = 0f;
		}

		UpdateHeight ();		
		UpdateRotation ();
	}

	void UpdateRotation()
	{
		yaw += yawSpeed * Input.GetAxis ("Mouse X") * Time.deltaTime;
		transform.eulerAngles = new Vector3(0,yaw,0);
	}

	bool IsGrounded()
	{
		RaycastHit hitInfo;
		return Physics.SphereCast (this.transform.position + playerColl.center, playerColl.radius * 0.9f, Vector3.down, out hitInfo, (height / 2) - playerColl.radius + 0.1f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
	}

	public void Move()
	{		
		//jumping
		if (input.GetButtonDown ("Jump") && !IsCeiled (heightSettings.maximumHeight * 1.1f)){
			if(canJumpOver) {
				highPointReached = false;
				isJumpingOver = true;
				highPoint = this.transform.position + movementSettings.jumpOverHeight * Vector3.up;
				rb.useGravity = false;
				JumpOver ();
				return;
			} else {
				isCrouching = false;
				Vector3 newVelocity = rb.velocity;
				newVelocity.y = 0f;
				rb.velocity = newVelocity;
				rb.AddForce (Vector3.up * movementSettings.jumpForce,ForceMode.Impulse);				
			}
		}

		rb.drag = 5f;

		//Crouching
		if (input.GetButtonDown ("Crouch"))
			ToggleCrouching ();

		//Running
		SetRunning (input.GetButton ("Run"));

		//calculate current speed
		currentSpeed = movementSettings.speed;

		if (isCrouching)
			currentSpeed *= movementSettings.crouchingMultiplier;
		if (isRunning)
			currentSpeed *= movementSettings.runningMultiplier;
		
		// Direction to move to
		UpdateDirection ();

		if( Mathf.Abs (direction.x) > float.Epsilon || Mathf.Abs (direction.z) > float.Epsilon)
		{
			if (rb.velocity.magnitude < currentSpeed)
				rb.AddForce (direction * currentSpeed * movementSettings.acceleration, ForceMode.Impulse);

			if (!isCrouching && !audioSource.isPlaying)
				audioSource.Play ();
		}
	}

	void JumpOver ()
	{
		float jumpingSpeed = 2f * Time.deltaTime;

		if(highPointReached) {
			this.transform.position = Vector3.MoveTowards (this.transform.position, landingPosition, jumpingSpeed);

			if (Vector3.Distance (this.transform.position, landingPosition) < 0.01f) {
				isJumpingOver = false;
				rb.useGravity = true;
			}
			
		} else {
			this.transform.position = Vector3.MoveTowards (this.transform.position, highPoint, jumpingSpeed);

			if (Vector3.Distance (this.transform.position, highPoint) < 0.01f)
				highPointReached = true;
		}
	}

	void UpdateDirection()
	{
		Vector3 relativeDestination = new Vector3();

		relativeDestination.x = input.GetAxis ("Horizontal");
		relativeDestination.z = input.GetAxis ("Vertical");

		direction = this.transform.TransformDirection (relativeDestination.normalized);

		foreach (Vector3 normal in contactNormals) {
			if( Mathf.Abs (Vector3.Angle (direction,normal)) > 90f ){
				direction = Vector3.ProjectOnPlane (direction, normal);
			}
		}

		contactNormals.Clear ();
	}

	void OnCollisionStay(Collision collisionInfo)
	{
		foreach(ContactPoint contact in collisionInfo.contacts) {
			contactNormals.Add (contact.normal);

			if(collisionInfo.gameObject.name != "Ground" && IsGrounded ()) {
				SearchJumpOverPosition (contact.normal);
			}
		}
	}

	void OnCollisionExit()
	{
		canJumpOver = false;
		uiManager.EraseText (UIManager.Labels.USE);
	}

	void SearchJumpOverPosition(Vector3 normal)
	{
		if(Mathf.Abs (Vector3.Angle (this.transform.forward, - normal)) < 45f 
			&& !Physics.Raycast (this.transform.position + movementSettings.jumpOverHeight * Vector3.up, - normal, playerColl.radius * 2)){

			RaycastHit hitInfo;

			if (Physics.Raycast (this.transform.position + movementSettings.jumpOverHeight * Vector3.up - normal * playerColl.radius * 2, -this.transform.up, out hitInfo, movementSettings.jumpOverHeight)) {
				landingPosition = hitInfo.point;
			} else {
				landingPosition = this.transform.position - normal * playerColl.radius * 2;
			}

			canJumpOver = true;
			uiManager.SetText (UIManager.Labels.USE, input.GetButtonAssignment ("Jump") + " : to climb");

		} else {
			canJumpOver = false;
			uiManager.EraseText (UIManager.Labels.USE);
		}
	}

	public void Die()
	{
		playerColl.isTrigger = true;
		this.enabled = false;
		this.GetComponentInChildren<ReachController> ().enabled = false;
		LevelManager.instance.Lose ();
		StartCoroutine ("DestroyOnTimer",1f);
	}

	IEnumerator DestroyOnTimer(float time)
	{
		yield return new WaitForSeconds (time);
		Deactivate ();
		level.mainCamAudio.enabled = true;
		Destroy (this.gameObject);
	}

	void ToggleCrouching()
	{
		if(!isCrouching || !IsCeiled(heightSettings.maximumHeight / 2 * 1.1f))
			isCrouching = !isCrouching;
	}

	bool IsCeiled(float height)
	{
		return Physics.Raycast (this.transform.position + playerColl.center, Vector3.up, height);
	}

	void SetRunning (bool _isRunning)
	{
		//if we are crouching or 
		if(!isCrouching || !IsCeiled (heightSettings.maximumHeight / 2 * 1.1f)){
			
			isRunning = _isRunning;

			if (isRunning)
				isCrouching = false;
		}
	}

	void UpdateHeight ()
	{
		if (isCrouching && height != heightSettings.minimumHeight)
			height = Mathf.MoveTowards (height, heightSettings.minimumHeight, heightSettings.crouchSpeed * Time.deltaTime);

		if (!isCrouching && height != heightSettings.maximumHeight)
			height = Mathf.MoveTowards (height, heightSettings.maximumHeight, heightSettings.crouchSpeed * Time.deltaTime);

		playerColl.height = height;
	}

	void MakeNoise ()
	{
		float soundReach = soundRadius;

		if (isRunning)
			soundReach *= movementSettings.runningMultiplier;

		Collider [] ears = Physics.OverlapSphere (this.transform.position, soundReach, enemyMask);

		foreach(Collider ear in ears){
			ear.SendMessage ("Hear",this.transform.position);
		}
	}
}
