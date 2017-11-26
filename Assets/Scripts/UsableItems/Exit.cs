using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class Exit : Usable {
	
	public override void Use(GameObject user)
	{
		LevelManager.instance.Win ();
	}
}
