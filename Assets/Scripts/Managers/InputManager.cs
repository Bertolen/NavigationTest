using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

	#region Singleton

	public static InputManager instance;

	void Awake()
	{
		if (instance != null)
		{
			Debug.LogWarning ("More than one instance of InputManager found!");
			Destroy (this.gameObject);
		}
		else{
			instance = this;
		}
	}

	#endregion

	Dictionary<string,string> buttons = new Dictionary<string,string> ();
	Dictionary<string,string[]> axes = new Dictionary<string,string[]>();

	void Start()
	{
		buttons.Add ("Forward",		"z");
		buttons.Add ("Backward",	"s");
		buttons.Add ("Left",		"q");
		buttons.Add ("Right",		"d");
		buttons.Add ("Jump",		"space");
		buttons.Add ("Use",			"f");
		buttons.Add ("KnockOut",	"mouse 0");
		buttons.Add ("Kill",		"mouse 1");
		buttons.Add ("Respawn",		"r");
		buttons.Add ("PeekLeft",	"a");
		buttons.Add ("PeekRight",	"e");
		buttons.Add ("Crouch",		"left ctrl");
		buttons.Add ("Run",			"left shift");
		buttons.Add ("Pause",		"escape");
		buttons.Add ("Submit",		"t");

		AddAxis ("Horizontal", 		"Right", 	"Left");
		AddAxis ("Vertical", 		"Forward", 	"Backward");
		AddAxis ("Peek",			"PeekRight","PeekLeft");
	}

	void AddAxis(string name, string positive, string negative)
	{
		if(axes.ContainsKey (name))
		{
			Debug.LogError ("Tried to add an existing axis : " + name);
			return;
		}

		if(!buttons.ContainsKey (positive))
		{
			Debug.LogError ("Tried to access an unknown button: " + positive);
			return;
		}

		if(!buttons.ContainsKey (negative))
		{
			Debug.LogError ("Tried to access an unknown button: " + negative);
			return;
		}

		string[] b = {positive,negative};

		axes.Add (name,b);
	}

	public float GetAxis(string name)
	{
		if(!axes.ContainsKey (name))
		{
			Debug.LogError ("Tried to acces unknown axis : " + name);
			return 0f;
		}

		float axisValue = 0f;
		string positive = axes [name] [0];
		string negative = axes [name] [1];

		if (GetButton (positive))
			axisValue++;
		
		if (GetButton (negative))
			axisValue--;

		return axisValue;
	}

	public bool GetButtonDown(string b)
	{
		if(buttons.ContainsKey (b))
		{
			return Input.GetKeyDown (buttons[b]);
		}
		Debug.LogError ("Tried to access unknown button : " + b);
		return false;
	}

	public bool GetButtonUp(string b)
	{
		if(buttons.ContainsKey (b))
		{
			return Input.GetKeyUp (buttons[b]);
		}
		Debug.LogError ("Tried to access unknown button : " + b);
		return false;
	}

	public bool GetButton(string b)
	{
		if(buttons.ContainsKey (b))
		{
			return Input.GetKey (buttons[b]);
		}
		Debug.LogError ("Tried to access unknown button : " + b);
		return false;
	}

	public bool ChangeButtonAssignment(string b, string k)
	{
		if(buttons.ContainsKey (b))
		{
			buttons[b] = k.ToLower ();
			return true;
		}
		Debug.LogError ("Tried to access unknown button : " + b);
		return false;
	}

	public string GetButtonAssignment(string b)
	{
		if(buttons.ContainsKey (b))
		{
			return buttons[b].ToUpper ();
		}
		Debug.LogError ("Tried to access unknown button : " + b);
		return "";
	}

	public List<string> GetButtonsNames()
	{
		return new List<string> (buttons.Keys);
	}
}
