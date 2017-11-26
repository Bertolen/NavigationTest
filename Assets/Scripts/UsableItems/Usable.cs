using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Usable : MonoBehaviour {

	bool available = true;

	public virtual void Use(GameObject user)
	{
		Debug.Log ("Using item : " + this.name);
	}

	public bool IsAvailable()
	{
		return available;
	}
}
