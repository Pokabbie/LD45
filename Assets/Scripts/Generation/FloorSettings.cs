using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomPlacement
{
	public float m_Weight = 1.0f;
	public RoomSettings m_Room;
}

[CreateAssetMenu]
public class FloorSettings : ScriptableObject
{
	[Header("General")]
	public string m_StageName = "Untitled Stage";
	public int m_MinRoomCount = 3;
	public int m_MaxRoomCount = 5;

	[Header("Rooms")]
	public RoomSettings m_SpawnRoom;
	public RoomSettings m_BossRoom;
	public RoomPlacement[] m_RoomTypes;

	[Header("Spawning")]
	public PlacementSettings m_EnemyPlacements;
	public PlacementSettings m_LootPlacements;
	
	public PlacementSettings m_WallDressingPlacements;
	public PlacementSettings m_FloorDressingPlacements;

	public RoomSettings SelectRandomRoom()
	{
		float totalWeight = 0.0f;
		for (int i = 0; i < m_RoomTypes.Length; ++i)
			totalWeight += m_RoomTypes[i].m_Weight;

		float weight = UnityEngine.Random.Range(0.0f, totalWeight);

		for (int i = 0; i < m_RoomTypes.Length - 1; ++i)
		{
			weight -= m_RoomTypes[i].m_Weight;
			if (weight <= 0.0f)
				return m_RoomTypes[i].m_Room;
		}

		return m_RoomTypes[m_RoomTypes.Length - 1].m_Room;
	}
}
