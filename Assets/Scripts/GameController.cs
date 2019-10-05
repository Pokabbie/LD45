using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
	private static GameController s_Main;
	public static GameController Main
	{
		get { return s_Main; }
	}

	[SerializeField]
	private GameObject m_PlayerPrefab;

	void Start()
	{
		if (s_Main == null)
			s_Main = this;
		else
			Debug.LogWarning("Multiple GameController found");

		SpawnPlayer();
	}

	private Vector3 GetPlayerSpawnPoint()
	{
		return Vector3.zero;
	}

	public void SpawnPlayer()
	{
		GameObject player = Instantiate(m_PlayerPrefab, GetPlayerSpawnPoint(), Quaternion.identity);
		player.tag = "Player";
		Camera.main.transform.position = player.transform.position;
	}
}
