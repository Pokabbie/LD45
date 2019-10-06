using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevelOnTrigger : MonoBehaviour
{
	void Awake()
	{
		transform.rotation = Quaternion.identity;
	}

	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
			GameController.Main.NextLevel();
	}
}
