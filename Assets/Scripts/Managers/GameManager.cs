using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	#region Singleton

	public static GameManager instance;

	void Awake()
	{
		if (instance != null) {
			Debug.LogWarning ("Found more than one instance of GameManager");
			Destroy (this.gameObject);
		} 
		else {
			instance = this;
			DontDestroyOnLoad (this);
		}
	}

	#endregion
}
