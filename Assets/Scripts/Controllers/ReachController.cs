using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReachController : MonoBehaviour {

	public GameObject character;
	public Transform POV;
	public float reachDst = 1.5f;
	public LayerMask interactableMask;

	InputManager input;
	GameObject item;
	GameObject enemy;
	UIManager uiManager;

	void Start()
	{
		input = InputManager.instance;
		uiManager = UIManager.instance;
	}

	void Update()
	{
		SearchInteractable ();

		if(character.CompareTag ("Player")){

			if (input.GetButtonDown ("Use")) {
				UseItem ();
			}

			if(input.GetButtonDown ("KnockOut")){
				KnockOutEnemy ();
			}
		}
	}

	void SearchInteractable ()
	{
		RaycastHit hitInfo;

		switch (character.tag) {
		case "Player":
			if (Physics.Raycast (POV.position, POV.forward, out hitInfo, reachDst, interactableMask.value)) {
				PlayerReachEnter (hitInfo.collider);
			} else {
				if (item != null) {
					RemoveItem ();
					uiManager.EraseText (UIManager.Labels.USE);
				}
				if (enemy != null) {
					RemoveEnemy ();
					uiManager.EraseText (UIManager.Labels.USE);
				}
			}
			break;

		case "Enemy":
			if (Physics.Raycast (POV.position - 1.1f * character.transform.up, POV.forward, out hitInfo, reachDst, interactableMask.value))
				EnemyReachEnter (hitInfo.collider);
			break;
		}
	}

	void PlayerReachEnter(Collider other)
	{
		if (other.CompareTag ("Usable")) {
			SetIem(other.gameObject);
		}

		if (other.CompareTag ("Enemy")) {
			SetEnemy (other.gameObject);
		}
	}

	void EnemyReachEnter(Collider other)
	{
		EnemyController.State state = character.GetComponent<EnemyController> ().GetState();
		if(state != EnemyController.State.SLEEP && other.CompareTag ("Player"))
		{
			other.gameObject.GetComponent<PlayerController> ().Die ();
		}
	}

	void PlayerReachExit(Collider other)
	{
		if (other.CompareTag ("Usable")) {
			RemoveItem ();
			uiManager.EraseText (UIManager.Labels.USE);
		}

		if (other.CompareTag ("Enemy")) {
			RemoveEnemy ();
			uiManager.EraseText (UIManager.Labels.USE);
		}
	}

	void EnemyReachExit(Collider other)
	{
		RemoveItem ();
	}

	void SetIem(GameObject newItem)
	{
		if (item == null) {
			item = newItem;
			uiManager.SetText(UIManager.Labels.USE,input.GetButtonAssignment ("Use") + " : use " + newItem.name);
		} else {
			float oldItemDst = Vector3.Distance (this.transform.position, item.transform.position);
			float newItemDst = Vector3.Distance (this.transform.position, newItem.transform.position);

			if (newItemDst < oldItemDst) {
				item = newItem;
				uiManager.SetText(UIManager.Labels.USE,input.GetButtonAssignment ("Use") + " : use " + newItem.name);
			}
		}
	}

	void RemoveItem()
	{
		item = null;
	}

	void UseItem()
	{
		if(item != null)
			item.GetComponent<Usable> ().Use (character);
	}

	void SetEnemy(GameObject newEnemy)
	{
		enemy = newEnemy;

		uiManager.SetText(UIManager.Labels.USE,input.GetButtonAssignment ("KnockOut") + " : knock-out enemy");
	}

	void RemoveEnemy()
	{
		enemy = null;
	}

	void KnockOutEnemy ()
	{
		if (enemy != null)
			enemy.GetComponent<EnemyController> ().KnockOut ();
	}
}
