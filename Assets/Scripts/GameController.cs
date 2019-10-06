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
	private GameObject m_GameOverScreen;
	[SerializeField]
	private GameObject m_PopupPrefab;

	[SerializeField]
	private GameObject m_PlayerPrefab;
	private GameObject m_CurrentPlayer;

	[SerializeField]
	private RoomInstance m_RoomPrefab;

	[SerializeField]
	private FloorSettings[] m_Floors;

	[SerializeField]
	private int m_ScoreOnKill = 50;
	[SerializeField]
	private int m_ScoreOnHit = 10;

	private int m_FloorIndex;
	private int m_Score;
	private FloorInstance m_CurrentFloor;

	void Start()
	{
		if (s_Main == null)
			s_Main = this;
		else
			Debug.LogWarning("Multiple GameController found");

		Restart();
	}
	
	public int CurrentScore
	{
		get { return m_Score; }
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

	public void Restart()
	{
		if (m_CurrentPlayer == null)
			m_CurrentPlayer = GameObject.FindGameObjectWithTag("Player");
		if (m_CurrentPlayer != null)
			Destroy(m_CurrentPlayer);

		m_Score = 0;
		m_FloorIndex = -1;
		NextLevel();
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

		StageNamePopup.Main.DisplayText(floorSettings.m_StageName, m_FloorIndex);
	}

	private void CreateFloor(FloorSettings floor)
	{
		m_CurrentFloor = FloorInstance.PlaceFloor(m_RoomPrefab, floor);
	}

	public void OnCharacterDamage(CharacterController character, bool isDead)
	{
		if (character.CompareTag("Player"))
		{
			if (isDead)
			{
				m_GameOverScreen.SetActive(true);
			}
		}
		else
		{
			int scoreReward = isDead ? m_ScoreOnKill : m_ScoreOnHit;
			CreateWorldspaceText("+" + scoreReward, character.transform.position, Color.white);
			m_Score += scoreReward;
		}
	}

	public WorldSpaceText CreateWorldspaceText(string message, Vector3 position, Color colour, float duration = 2.0f)
	{
		return WorldSpaceText.CreatePopup(m_PopupPrefab, message, position, colour, duration);
	}
}
