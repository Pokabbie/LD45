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
	private GameObject m_CurrentPlayer;

	[SerializeField]
	private RoomInstance m_RoomPrefab;

	[SerializeField]
	private FloorSettings[] m_Floors;

	private int m_FloorIndex;
	private FloorInstance m_CurrentFloor;

	void Start()
	{
		if (s_Main == null)
			s_Main = this;
		else
			Debug.LogWarning("Multiple GameController found");

		m_FloorIndex = -1;
		NextLevel();
	}

	public bool DEBUGNEXLVL;
	void Update()
	{
		if (DEBUGNEXLVL)
		{
			DEBUGNEXLVL = false;
			NextLevel();
		}
	}

	private Vector3 GetPlayerSpawnPoint()
	{
		return Vector3.zero;
	}

	public void SpawnPlayer()
	{
		if (m_CurrentPlayer == null)
			m_CurrentPlayer = GameObject.FindGameObjectWithTag("Player");

		if (m_CurrentPlayer == null)
		{
			m_CurrentPlayer = Instantiate(m_PlayerPrefab, GetPlayerSpawnPoint(), Quaternion.identity);
			m_CurrentPlayer.tag = "Player";
		}
		else
			m_CurrentPlayer.transform.position = GetPlayerSpawnPoint();

		Camera.main.transform.position = m_CurrentPlayer.transform.position;
	}

	public void NextLevel()
	{
		m_FloorIndex++;
		Debug.Log("Level " + m_FloorIndex);

		if (m_CurrentFloor != null)
			m_CurrentFloor.Cleanup();

		FloorSettings floorSettings = m_Floors[Mathf.Min(m_FloorIndex, m_Floors.Length - 1)];
		m_CurrentFloor = FloorInstance.PlaceFloor(m_RoomPrefab, floorSettings);

		SpawnPlayer();
	}

	private void CreateFloor(FloorSettings floor)
	{
		m_CurrentFloor = FloorInstance.PlaceFloor(m_RoomPrefab, floor);
	}
}
