using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Closet : Usable {

	public float angularSpeed = 90;
	public float speed = 0.5f;
	public Transform outside;
	public Transform inside;

	GameObject user = null;
	Collider userCollider;
	Rigidbody userRB;
	bool goingIn = false;
	bool full = false;
	bool goingOut = false;

	public override void Use(GameObject _user)
	{
		if(goingIn||goingOut){
			return;
		}

		if (user == null){
			CaptureUser (_user);
			goingIn = true;
		}
		else if (full){
			goingOut = true;
		}
	}

	void Update()
	{
		if (!full || user == null)
			return;

		if (goingIn)
			GoIn ();
		else if (goingOut)
			GoOut ();
	}

	void CaptureUser(GameObject _user)
	{
		user = _user;
		user.GetComponent<PlayerController> ().enabled = false;
		full = true;
		user.transform.rotation = outside.rotation;
		user.transform.position = outside.position;
		userCollider = user.GetComponent<Collider> ();
		userRB = user.GetComponent<Rigidbody> ();
		userCollider.isTrigger = true;
		userRB.useGravity = false;
	}

	void ReleaseUser()
	{
		full = false;
		userCollider.isTrigger = false;
		user.GetComponent<PlayerController> ().ResetYaw();
		user.GetComponent<PlayerController> ().enabled = true;
		userRB.useGravity = true;
		user = null;	
	}
		
	void GoOut()
	{
		user.transform.position = Vector3.MoveTowards (user.transform.position, outside.position, speed * Time.deltaTime);

		if(user.transform.position == outside.position ){
			ReleaseUser ();
			goingOut = false;
		}
	}

	void GoIn()
	{
		user.transform.rotation = Quaternion.RotateTowards (user.transform.rotation, inside.rotation, angularSpeed * Time.deltaTime);
		user.transform.position = Vector3.MoveTowards (user.transform.position, inside.position, speed * Time.deltaTime);

		if(user.transform.position == inside.position && user.transform.rotation == inside.rotation){
			goingIn = false;
		}
	}
}
