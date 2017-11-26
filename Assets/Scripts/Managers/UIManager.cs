using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	#region Singleton

	public static UIManager instance;

	void Awake()
	{
		if (instance != null)
			Debug.LogWarning ("Found more than one instance of UIManager");

		instance = this;
	}

	#endregion

	public enum Labels{
		STATE,
		USE,
		SUBSTATE
	}

	public Text[] texts;

	void Start()
	{
		HideAllTexts ();
	}

	public void HideAllTexts()
	{
		for (int i = 0; i < texts.Length; i++)
		{
			texts [i].enabled = false;
		}
	}

	public void SetText(Labels textLabel, string text)
	{
		int i = (int)textLabel;
		texts [i].enabled = true;
		texts [i].text = text;
	}

	public void EraseText(Labels textLabel)
	{
		int i = (int)textLabel;
		texts [i].enabled = false;
		texts [i].text = "";
	}
}
