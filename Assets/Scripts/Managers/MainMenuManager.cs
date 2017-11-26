using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {

	public GameObject LevelSelectionMenu;

	public void OnPlayButton()
	{
		this.gameObject.SetActive (false);
		LevelSelectionMenu.SetActive (true);
	}

	public void OnOptionsButton()
	{
		Debug.Log ("Accessing options");
	}

	public void OnQuitButton()
	{
		Application.Quit ();
	}
}
