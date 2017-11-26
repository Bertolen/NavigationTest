using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectionManager : MonoBehaviour {

	public GameObject MainMenu;

	public void OnBackButton()
	{
		this.gameObject.SetActive (false);
		MainMenu.SetActive (true);
	}

	public void OnLvl001Button()
	{
		SceneManager.LoadScene ("Level001");
	}

	public void OnLvl002Button()
	{
		SceneManager.LoadScene ("Level002");
	}
}
