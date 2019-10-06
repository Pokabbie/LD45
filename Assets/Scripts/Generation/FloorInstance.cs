using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorInstance : MonoBehaviour
{
	private FloorSettings m_FloorSettings;

	[SerializeField]
	private RoomInstance m_RoomPrefab;

	public bool SPAWNER = false;
	public FloorSettings TESTSETTINGS;

	// Start is called before the first frame update
	void Start()
	{
		if(SPAWNER)
			PlaceFloor(m_RoomPrefab, TESTSETTINGS);
	}
	

	public static FloorInstance PlaceFloor(RoomInstance roomPrefab, FloorSettings floorSettings)
	{
		FloorInstance instance = new GameObject().AddComponent<FloorInstance>();
		instance.gameObject.name = "Floor";
		instance.m_FloorSettings = floorSettings;
		instance.m_RoomPrefab = roomPrefab;

		List<PlacedRoomInfo> roomInfos = new List<PlacedRoomInfo>();

		int roomCount = Random.Range(floorSettings.m_MinRoomCount, floorSettings.m_MaxRoomCount);
		for (int i = 0; i < roomCount; ++i)
			roomInfos.Add(instance.NewRoomInfo(roomInfos, roomCount));

		
		foreach (PlacedRoomInfo info in roomInfos)
			instance.CreateRoom(info);

		return instance;
	}

	private PlacedRoomInfo NewRoomInfo(List<PlacedRoomInfo> roomInfos, int totalCount)
	{
		RoomSettings settings;

		if (roomInfos.Count == 0)
		{
			settings = m_FloorSettings.m_SpawnRoom;
			Vector3Int centre = new Vector3Int();
			Vector3Int extents = settings.RandomExtents();
			return new PlacedRoomInfo(settings, centre, extents);
		}
		else if (roomInfos.Count == totalCount - 1)
			settings = m_FloorSettings.m_BossRoom;
		else
			settings = m_FloorSettings.SelectRandomRoom();
		
		return PlacedRoomInfo.FindRoomSlot(settings, roomInfos);
	}

	private RoomInstance CreateRoom(PlacedRoomInfo roomInfo)
	{
		RoomInstance instance = RoomInstance.PlaceRoom(m_RoomPrefab, m_FloorSettings, roomInfo.m_Settings, roomInfo.m_Centre, roomInfo.m_Extents, roomInfo.m_Connection);
		instance.transform.parent = transform;
		return instance;
	}
}

internal class PlacedRoomInfo
{
	public RoomSettings m_Settings;
	public Vector3Int m_Centre;
	public Vector3Int m_Extents;
	public RoomConnections m_Connection;

	public PlacedRoomInfo(RoomSettings settings, Vector3Int centre, Vector3Int extents, RoomConnections connection = RoomConnections.None)
	{
		m_Settings = settings;
		m_Centre = centre;
		m_Extents = extents;
		m_Connection = connection;
	}

	public bool Overlaps(Vector3Int centre, Vector3Int extents)
	{
		BoundsInt thisBounds = new BoundsInt(m_Centre, m_Extents);
		BoundsInt otherBounds = new BoundsInt(centre, extents);
		return !(thisBounds.xMax <= otherBounds.xMin || thisBounds.xMin >= otherBounds.xMax
			|| thisBounds.yMax <= otherBounds.yMin || thisBounds.yMin >= otherBounds.yMax
			|| thisBounds.zMax <= otherBounds.zMin || thisBounds.zMin >= otherBounds.zMax
			);
	}

	public static PlacedRoomInfo FindRoomSlot(RoomSettings settings, List<PlacedRoomInfo> allRooms)
	{
		Vector3Int centre = new Vector3Int();
		Vector3Int extents = settings.RandomExtents();

		int randomIndex = Random.Range(0, allRooms.Count - 1);
		for (int i = 0; i < allRooms.Count; ++i)
		{
			int index = (randomIndex + i) % allRooms.Count;

			RoomConnections connection = allRooms[index].FindOpenSlot(allRooms, ref centre, extents);
			if (connection != RoomConnections.None)
				return new PlacedRoomInfo(settings, centre, extents, connection);
		}

		Debug.Assert(false, "Failed to find a room slot!");
		return null;
	}

	private RoomConnections FindOpenSlot(List<PlacedRoomInfo> allRooms, ref Vector3Int centre, Vector3Int extents)
	{
		if (m_Connection != RoomConnections.All)
		{
			int randInt = Random.Range(0, 3);

			for (int i = 0; i < 4; ++i)
			{
				int dir = (i + randInt) % 4;

				switch (dir)
				{
					case 0:
						if (CheckForOpenSlot(allRooms, RoomConnections.Left, ref centre, extents))
							return RoomConnections.Right;
						break;
					case 1:
						if (CheckForOpenSlot(allRooms, RoomConnections.Right, ref centre, extents))
							return RoomConnections.Left;
						break;
					case 2:
						if (CheckForOpenSlot(allRooms, RoomConnections.Top, ref centre, extents))
							return RoomConnections.Bottom;
						break;
					case 3:
						if (CheckForOpenSlot(allRooms, RoomConnections.Bottom, ref centre, extents))
							return RoomConnections.Top;
						break;
				}
			}
		}

		return RoomConnections.None;
	}

	private bool CheckForOpenSlot(List<PlacedRoomInfo> allRooms, RoomConnections connection, ref Vector3Int centre, Vector3Int extents)
	{
		if ((m_Connection & connection) != 0)
			return false;

		switch (connection)
		{
			case RoomConnections.Top:
				centre = m_Centre + new Vector3Int(0, 0, (m_Extents.z + extents.z) / 2);
				break;
			case RoomConnections.Bottom:
				centre = m_Centre - new Vector3Int(0, 0, (m_Extents.z + extents.z) / 2);
				break;
			case RoomConnections.Right:
				centre = m_Centre + new Vector3Int((m_Extents.x + extents.x) / 2, 0, 0);
				break;
			case RoomConnections.Left:
				centre = m_Centre - new Vector3Int((m_Extents.x + extents.x) / 2, 0, 0);
				break;
		}

		foreach (PlacedRoomInfo roomInfo in allRooms)
		{
			if (roomInfo != this && roomInfo.Overlaps(centre, extents))
				return false;
		}

		m_Connection |= connection;
		return true;
	}
}
