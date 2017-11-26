using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

[RequireComponent(typeof(AudioSource))]
public class LevelManager : MonoBehaviour {

	#region Singleton

	public static LevelManager instance;

	void Awake()
	{
		if (instance != null)
			Debug.LogWarning ("Found more than one instance of LevelManager");

		instance = this;
	}

	#endregion

	public Text stateText;
	public Transform spawnPoint;
	public GameObject PlayerPrefab;
	public Transform target;
	public RectTransform marker;
	public int bordersOffset = 16;
	public AudioClip victorySFX;
	public AudioClip defeatSFX;
	public AudioListener mainCamAudio;

	GameObject player = null;
	UIManager uiManager;
	InputManager input;
	AudioSource audioSource;
	EnemyController[] enemies;
	bool won;
	List<string> scenesInBuild;

	void Start()
	{
		enemies = FindObjectsOfType<EnemyController> ();
		marker.gameObject.SetActive (true);
		audioSource = GetComponent<AudioSource> ();
		uiManager = UIManager.instance;
		input = InputManager.instance;
		won = false;
		scenesInBuild = new List<string> ();
		GetScenesInBuild ();
		HideCursor ();
		SpawnPlayer ();
	}

	void GetScenesInBuild ()
	{
		for(int i = 1 ; i < SceneManager.sceneCountInBuildSettings; i++){
			string scenePath = SceneUtility.GetScenePathByBuildIndex (i);
			int lastSlash = scenePath.LastIndexOf ("/");
			scenesInBuild.Add (scenePath.Substring (lastSlash + 1, scenePath.LastIndexOf (".") - lastSlash - 1));
		}
	}

	void Update()
	{
		if(player == null && input.GetButtonDown ("Respawn"))
		{
			SpawnPlayer ();
			ResetAllEnemies ();
			uiManager.HideAllTexts ();
		}

		if(won && input.GetButtonDown ("Submit"))
		{
			LoadNextScene ();
		}

		DisplayTargetMarker ();
	}

	public void Lose()
	{
		audioSource.clip = defeatSFX;
		audioSource.Play ();
		uiManager.SetText (UIManager.Labels.STATE,"GAME OVER");
		uiManager.SetText (UIManager.Labels.SUBSTATE,"Press " + input.GetButtonAssignment ("Respawn") + " to respawn");
		uiManager.EraseText (UIManager.Labels.USE);
	}

	public void Win()
	{
		won = true;
		DeactivateAllEnemies ();
		audioSource.clip = victorySFX;
		audioSource.Play ();
		uiManager.SetText (UIManager.Labels.STATE, "YOU WON");
		uiManager.SetText (UIManager.Labels.SUBSTATE,"Press " + input.GetButtonAssignment ("Submit"));
		uiManager.EraseText (UIManager.Labels.USE);
		player.GetComponent<PlayerController> ().Deactivate ();
	}

	public void HideCursor()
	{
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Confined;
	}

	public void RestoreCursor()
	{
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public bool SpawnPlayer()
	{
		if (player == null) {
			player = (GameObject)Instantiate (PlayerPrefab, spawnPoint.position, spawnPoint.rotation);
			player.transform.rotation = spawnPoint.rotation;
			mainCamAudio.enabled = false;
			audioSource.Stop ();
			return true;
		}
		else{
			Debug.LogWarning ("Tried to instantiate more than one player!");
			return false;
		}
	}

	void ResetAllEnemies ()
	{
		foreach (EnemyController enemy in enemies)
		{
			enemy.Reset ();
		}
	}

	public void DeactivateAllEnemies ()
	{
		foreach (EnemyController enemy in enemies)
		{
			enemy.Deactivate ();
		}
	}

	public void ActivateAllEnemies ()
	{
		foreach (EnemyController enemy in enemies)
		{
			enemy.Activate ();
		}
	}

	public CapsuleCollider GetPlayerCollider ()
	{
		if (player != null)
			return player.GetComponent<CapsuleCollider> ();
		return null;
	}

	public PlayerController GetPlayerController ()
	{
		if (player != null)
			return player.GetComponent<PlayerController> ();
		return null;
	}

	public void SetTarget(Transform _target)
	{
		target = _target;
	}

	void DisplayTargetMarker()
	{
		if(player != null)
		{
			Camera cam = player.GetComponentInChildren<Camera> ();
			Vector3 screenPosition = cam.WorldToScreenPoint (target.position);

			if (screenPosition.z < 0f)
				screenPosition *= -1f;

			screenPosition.x = Mathf.Clamp (screenPosition.x, bordersOffset, cam.pixelWidth - bordersOffset);
			screenPosition.y = Mathf.Clamp (screenPosition.y, bordersOffset, cam.pixelHeight - bordersOffset);

			marker.position = screenPosition;
		}
	}

	void LoadNextScene ()
	{
		/*
		int nextSceneIndex = SceneManager.GetActiveScene ().buildIndex + 1;

		if(nextSceneIndex < SceneManager.sceneCountInBuildSettings){
			SceneManager.LoadSceneAsync (nextSceneIndex);
		} else {
			SceneManager.LoadSceneAsync ("MainMenu");
		}
		*/

		string currentScene = SceneManager.GetActiveScene ().name;
		int nextIndex = Int32.Parse (currentScene.Substring (5, 3)) + 1;

		Debug.Log (nextIndex);

		string nextScene = "Level" + nextIndex.ToString ("000");

		if(scenesInBuild.Contains (nextScene)){
			SceneManager.LoadSceneAsync (nextScene);
		} else {
			SceneManager.LoadSceneAsync ("MainMenu");
		}
	}
}
