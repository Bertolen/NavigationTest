using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextTrigger : MonoBehaviour {

	public UIManager.Labels uiText;
	public string text;
	public float CharactersPerSecond = 10f;

	UIManager uiManager;
	InputManager inputManager;
	float charactersDisplayed;
	bool isDisplaying = false;
	string displayedText;

	void Start()
	{
		uiManager = UIManager.instance;
		inputManager = InputManager.instance;
		charactersDisplayed = 0;
	}

	void FixedUpdate()
	{
		if(isDisplaying){

			if(charactersDisplayed < displayedText.Length){
				CalculateDisplayedCharacters ();
			}

			DisplayText ();
		}
	}

	void OnTriggerEnter()
	{
		displayedText = ConvertRawTextToInput (text);
		isDisplaying = true;
	}

	void OnTriggerExit()
	{
		uiManager.EraseText (uiText);
		isDisplaying = false;
	}

	string ConvertRawTextToInput(string text)
	{
		string result = "";
		string[] split = text.Split ('*');

		for (int i = 0 ; i < split.Length ; i++){
		
			if(i % 2 == 0){
				result += split [i];
			} else {
				result += inputManager.GetButtonAssignment (split [i]);
			}
		}

		return result;
	}

	void CalculateDisplayedCharacters ()
	{
		charactersDisplayed += CharactersPerSecond * Time.deltaTime;
	}

	void DisplayText ()
	{
		uiManager.SetText (uiText, displayedText.Substring (0, (int) charactersDisplayed));		
	}
}
