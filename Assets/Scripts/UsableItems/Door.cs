using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Usable {

	public Transform rotationAxis;
	public float openingTime = 1f;
	public float closedAngle = 0f;
	public float openAngle = 90f;

	bool isRotating = false;
	bool isOpen = false;
	float timer;
	Quaternion closedRotation;
	Quaternion openRotation;

	void Start()
	{
		closedRotation = rotationAxis.rotation * Quaternion.AngleAxis (closedAngle, rotationAxis.up);
		openRotation = rotationAxis.rotation * Quaternion.AngleAxis (openAngle, rotationAxis.up);
	}

	public override void Use(GameObject _user)
	{
		if(!isRotating){
			isRotating = true;
			timer = 0f;
		}
	}

	void FixedUpdate()
	{
		if(isRotating){
			if(isOpen){
				Rotate (openRotation, closedRotation);
			}else{
				Rotate (closedRotation, openRotation);
			}

			if (timer >= openingTime) {
				isOpen = !isOpen;
				isRotating = false;
			}
		}
	}

	void Rotate(Quaternion startingRotation, Quaternion targetRotation)
	{
		timer += Time.deltaTime;

		rotationAxis.rotation = Quaternion.Lerp (startingRotation, targetRotation, timer / openingTime);
	}
}