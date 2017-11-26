using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

	public GameObject PauseMenuUI;

	LevelManager level;
	InputManager input;

	void Start()
	{
		input = InputManager.instance;
		level = LevelManager.instance;
	}

	void Update()
	{
		if(input.GetButtonDown ("Pause")) {
			ToggleOnOff ();
		}
	}

	void ToggleOnOff()
	{
		if(PauseMenuUI.gameObject.activeSelf) {
			TurnOff ();
		}
		else {
			TurnOn ();
		}
	}

	void TurnOff()
	{
		PauseMenuUI.gameObject.SetActive (false);
		level.HideCursor ();
		//level.GetPlayerController ().Activate ();
		level.HideCursor ();
		level.ActivateAllEnemies ();
		Time.timeScale = 1f;
	}

	void TurnOn()
	{
		PauseMenuUI.gameObject.SetActive (true);
		level.RestoreCursor ();
		//level.GetPlayerController ().Deactivate ();
		level.RestoreCursor ();
		level.DeactivateAllEnemies ();
		Time.timeScale = 0f;
	}

	public void OnResumeButtonClick()
	{
		ToggleOnOff ();
	}

	public void OnOptionsButtonClick()
	{
		Debug.Log ("Accessing options");
	}

	public void OnQuitButtonClick()
	{
		SceneManager.LoadScene ("MainMenu");
	}
}
