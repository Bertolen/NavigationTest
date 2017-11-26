using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class EnemyController : MonoBehaviour {

	public Transform patrolPath;
	public int startingWaypoint = 1;
	public float FOVDepth = 15f;
	public float FOVHorizontalAngle = 40f;
	public float FOVVerticalAngle = 85f;
	public LayerMask obstacleMask;
	public float sleepTime = 2f;
	public float walkingSpeed = 2.5f;
	public float runningMultiplier = 2f;
	public float investigateTime = 5f;
	public float hearingDstOverride = -1f;

	public enum State{
		PATROL,
		CHASE,
		INVESTIGATE,
		SLEEP
	}

	State state;
	NavMeshAgent agent;
	int nextWaypoint;
	Transform[] waypoints;
	bool alive = true;
	Vector3 lastKnownPlayerPosition;
	float timer;
	AudioSource audioSource;
	FOVController fovController;
	Vector3 clue;
		
	void Start()
	{
		agent = GetComponent<NavMeshAgent> ();
		audioSource = GetComponent<AudioSource> ();
		fovController = GetComponentInChildren<FOVController> ();

		waypoints = new Transform[patrolPath.childCount];
		for (int i = 0 ; i < waypoints.Length ; i++) 
		{
			waypoints [i] = patrolPath.GetChild (i);
		}

		nextWaypoint = startingWaypoint;
		fovController.farDst = FOVDepth;
		fovController.horizontalAngle = FOVHorizontalAngle;
		fovController.verticalAngle = FOVVerticalAngle;
		StartPatrol ();
		
		StartCoroutine ("FiniteStateMachine");
	}

	IEnumerator FiniteStateMachine()
	{
		while (alive)
		{
			switch(state)
			{
			case State.PATROL:
				Patrol ();
				break;
			case State.CHASE:
				Chase ();
				break;
			case State.INVESTIGATE:
				Investigate ();
				break;
			case State.SLEEP:
				Sleep ();
				break;
			}
			yield return null;
		}
	}

	void LateUpdate() {
		if (state != State.SLEEP) {
			SearchForPlayer ();
		}
	}

	void SearchForPlayer ()
	{
		CapsuleCollider playerCollider = LevelManager.instance.GetPlayerCollider ();
		Plane[] planes = fovController.CalculateFrustumPlanes ();

		if(playerCollider != null && GeometryUtility.TestPlanesAABB (planes,playerCollider.bounds)) {
			Vector3 center = playerCollider.transform.position + playerCollider.center;
			Vector3 eyes = center + playerCollider.bounds.extents.y * Vector3.up;
			bool playerFound = CheckForPlayer (eyes);

			if(playerFound){
				if (state != State.CHASE) {
					StartChase ();
				} else {
					UpdateLastKnownPlayerPosition ();
				}
			}
		}
	}

	bool CheckForPlayer(Vector3 target)
	{
		Vector3 dirToTarget = (target - fovController.transform.position).normalized;
		float dstToTarget = Vector3.Distance (fovController.transform.position, target);

		if(!Physics.Raycast (fovController.transform.position, dirToTarget, dstToTarget, obstacleMask))
		{
			Debug.DrawRay (fovController.transform.position, dirToTarget * dstToTarget, Color.red);
			return true;
		}

		Debug.DrawRay (fovController.transform.position, dirToTarget * dstToTarget, Color.green);
		return false;
	}

	void UpdateLastKnownPlayerPosition ()
	{
		lastKnownPlayerPosition = LevelManager.instance.GetPlayerCollider ().transform.position;
		agent.SetDestination (lastKnownPlayerPosition);
	}

	public void Reset()
	{
		agent.enabled = false;
		nextWaypoint = startingWaypoint;
		this.transform.SetPositionAndRotation (waypoints[nextWaypoint].position,waypoints[nextWaypoint].rotation);
		agent.enabled = true;
		StartPatrol ();
		Debug.Log ("Enemy Reset");
	}

	public void Activate()
	{
		this.enabled = true;
		agent.enabled = true;
		StartCoroutine ("FiniteStateMachine");
	}

	public void Deactivate()
	{
		this.enabled = false;
		agent.enabled = false;
		StopCoroutine ("FiniteStateMachine");
	}

	void Patrol()
	{
		if(CheckDestination ()){
			if(CheckRotation()){
				GoToNextWaypoint ();
			}
			else{
				RotateToWaypoint ();
			}
		}
	}

	void StartPatrol()
	{
		this.GetComponentInChildren<Renderer> ().material.color = Color.black; //To be removed

		state = State.PATROL;
		agent.speed = walkingSpeed;

		GoToNextWaypoint ();
	}
		
	void GoToNextWaypoint()
	{
		UpdateNextWaypoint ();
		agent.SetDestination (waypoints[nextWaypoint].position);
	}

	bool CheckDestination ()
	{
		return (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f));
	}

	bool CheckRotation()
	{
		return (this.transform.rotation == waypoints [nextWaypoint].rotation);
	}

	void RotateToWaypoint()
	{
		this.transform.rotation = Quaternion.RotateTowards (this.transform.rotation, waypoints [nextWaypoint].rotation, agent.angularSpeed * Time.deltaTime);
	}

	void UpdateNextWaypoint()
	{
		nextWaypoint++;

		if (nextWaypoint == waypoints.Length)
			nextWaypoint = 0;
	}

	int CloserWaypoint()
	{
		int closerWaypoint = 0;
		float minDistance = Vector3.Distance (this.transform.position, waypoints [0].position);
		float distance;

		for (int i = 1; i < waypoints.Length; i++)
		{
			distance = Vector3.Distance (this.transform.position, waypoints [i].position);

			if (distance < minDistance)
			{
				minDistance = distance;
				closerWaypoint = i;
			}
		}

		return closerWaypoint;
	}

	void StartChase()
	{
		this.GetComponentInChildren<Renderer> ().material.color = Color.red; //To be removed
		state = State.CHASE;
		agent.speed = walkingSpeed * runningMultiplier;
		UpdateLastKnownPlayerPosition ();
	}

	void Chase()
	{
		bool endOfTrail = CheckDestination ();
		if (endOfTrail) {
			nextWaypoint = CloserWaypoint ();
			StartPatrol ();
		}
	}

	public void KnockOut()
	{
		state = State.SLEEP;
		timer = 0f;
		agent.enabled = false;
		this.GetComponentInChildren<Renderer> ().material.color = Color.blue; //To be removed
		audioSource.Play ();
	}

	void Sleep()
	{
		timer += Time.deltaTime;

		if(timer > sleepTime)
		{
			agent.enabled = true;
			StartPatrol ();
		}
	}

	public void Hear(Vector3 location)
	{
		if (state == State.SLEEP)
			return;
		
		if (hearingDstOverride > 0 && Vector3.Distance (location,this.transform.position) >= hearingDstOverride){
			return;
		}

		clue = location;
		state = State.INVESTIGATE;
		agent.ResetPath ();
		timer = 0f;
	}

	void Investigate()
	{
		if(!LookingAtClue()){
			LookAtClue ();
		}		

		timer += Time.deltaTime;

		if (timer >= investigateTime){
			StartPatrol ();
		}
	}

	bool LookingAtClue()
	{
		Vector3 differential = new Vector3 (clue.x, this.transform.position.y, clue.z) - this.transform.position;
		float angle = Quaternion.Angle(this.transform.rotation, Quaternion.LookRotation (differential));

		return (angle <= 5f);
	}

	void LookAtClue()
	{
		this.transform.rotation = Quaternion.RotateTowards (this.transform.rotation, Quaternion.LookRotation (clue - this.transform.position), agent.angularSpeed * Time.deltaTime);
	}

	public State GetState()
	{
		return	state;
	}
}
