using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class RoomInstance : MonoBehaviour
{
	[SerializeField]
	private GameObject m_GatePrefab;

	[SerializeField]
	private bool m_IsBossRoom;
	private Vector3Int m_Extents;
	private RoomConnections m_Connections;

	private Vector3 m_TopDoorLocation;
	private Vector3 m_BottomDoorLocation;
	private Vector3 m_LeftDoorLocation;
	private Vector3 m_RightDoorLocation;

	private GameObject m_EnemyContentContainer;
	private GameObject m_FloorContentContainer;
	private GameObject m_WallContentContainer;

	private FloorSettings m_FloorSettings;
	private RoomSettings m_RoomSettings;
	
	private int m_WaitTime = 0;

	void Start()
	{
		m_WaitTime = 4;
	}

	void Update()
	{
		if (m_WaitTime-- == 0)
		{
			m_FloorContentContainer.SetActive(true);
			m_WallContentContainer.SetActive(true);

			if (!m_IsBossRoom)
				m_EnemyContentContainer.SetActive(true);
		}

		if (m_IsBossRoom && !m_EnemyContentContainer.activeInHierarchy)
		{
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			if (player != null)
			{
				float spawnDistance = Mathf.Min(m_Extents.x, m_Extents.z) * 0.5f - 5.0f;

				float playerDistSq = (player.transform.position - transform.position).sqrMagnitude;

				if(playerDistSq <= spawnDistance * spawnDistance)
					m_EnemyContentContainer.SetActive(true);
			}
		}
	}

	private void SetupRoom(RoomGeneratorHelper generator, Vector3Int extents)
	{
		m_Extents = extents;

		GetComponent<MeshFilter>().mesh = generator.GeneratedMesh;
		GetComponent<MeshCollider>().sharedMesh = generator.GeneratedMesh;

		Vector2Int doorLoc;
		if (generator.GetDoor(RoomConnections.Top, out doorLoc))
			m_TopDoorLocation = transform.position + new Vector3(0.0f, m_RoomSettings.m_FloorHeight -1.0f, m_Extents.z * 0.5f);
		if (generator.GetDoor(RoomConnections.Bottom, out doorLoc))
			m_BottomDoorLocation = transform.position + new Vector3(0.0f, m_RoomSettings.m_FloorHeight - 1.0f, m_Extents.z * -0.5f);
		if (generator.GetDoor(RoomConnections.Left, out doorLoc))
			m_LeftDoorLocation = transform.position + new Vector3(m_Extents.x * -0.5f, m_RoomSettings.m_FloorHeight - 1.0f, 0.0f);
		if (generator.GetDoor(RoomConnections.Right, out doorLoc))
			m_RightDoorLocation = transform.position + new Vector3(m_Extents.x * 0.5f, m_RoomSettings.m_FloorHeight - 1.0f, 0.0f);

		// Setup contains to make management easier for spawned stuff
		m_EnemyContentContainer = new GameObject();
		m_EnemyContentContainer.transform.parent = transform;
		m_EnemyContentContainer.SetActive(false);

		m_FloorContentContainer = new GameObject();
		m_FloorContentContainer.transform.parent = transform;
		m_FloorContentContainer.SetActive(false);

		m_WallContentContainer = new GameObject();
		m_WallContentContainer.transform.parent = transform;
		m_WallContentContainer.SetActive(false);

		ProcessPlacementSettings(m_EnemyContentContainer, generator, GetEnemyPlacementSettings(), new Vector3(0, 1, 0), false, true);
		ProcessPlacementSettings(m_FloorContentContainer, generator, GetLootPlacementSettings(), new Vector3(0, 1, 0), true, true);
		ProcessPlacementSettings(m_FloorContentContainer, generator, GetFloorDressingSettings(), new Vector3(0, 0, 0), true, true);
		ProcessPlacementSettings(m_WallContentContainer, generator, GetWallDressingSettings(), new Vector3(0, 0, 0), true, false);

		// Only make barrier on one side
		if (HasTopDoor)
			Instantiate(m_GatePrefab, m_TopDoorLocation, Quaternion.identity, transform);
		if (HasLeftDoor)
			Instantiate(m_GatePrefab, m_LeftDoorLocation, Quaternion.AngleAxis(90.0f, Vector3.up), transform);
	}

	private void ProcessPlacementSettings(GameObject targetContainer, RoomGeneratorHelper generator, PlacementSettings settings, Vector3 placeOffset, bool randomRotations, bool useAccessibleSlots)
	{
		int placeCount = Random.Range(settings.m_MinPlacements, settings.m_MaxPlacements);

		if (targetContainer == m_EnemyContentContainer && m_IsBossRoom)
			placeCount = 1 + GameController.Main.CurrentLevel * 2;

		if (m_IsBossRoom) //?
			placeOffset += new Vector3(0, -1.62f, 0);

		for (int n = 0; n < placeCount; ++n)
		{
			bool hasSlots = useAccessibleSlots ? generator.AccessibleSpots.Any() : generator.InaccessibleSpots.Any();
			if (!hasSlots)
				return;

			GroupPlacementSettings groupSettings = settings.SelectRandomGroup();
			int objectCount = Random.Range(groupSettings.m_MinElements, groupSettings.m_MaxElements) + groupSettings.m_AlwaysPlacedObjects.Length;

			var spots = useAccessibleSlots ? generator.GetFreeAccessibleSpot(objectCount) : generator.GetFreeInaccessibleSpot(objectCount);

			for (int i = 0; i < spots.Count(); ++i)
			{
				GameObject targetObj;

				if (i < groupSettings.m_AlwaysPlacedObjects.Length)
					targetObj = groupSettings.m_AlwaysPlacedObjects[i];
				else
					targetObj = groupSettings.SelectRandomObject().m_PlacedObject;

				Vector3 offset = new Vector3(Random.Range(-1.0f, 1.0f), -1.0f, Random.Range(-1.0f, 1.0f)) + placeOffset;
				float angle = randomRotations ? Random.Range(0, 360f) : 0.0f;

				Vector3 spawnPosition = transform.position + offset + spots.ElementAt(i) - new Vector3(m_Extents.x, 0, m_Extents.z) * 0.5f;
				GameObject newObj = Instantiate(targetObj, spawnPosition, Quaternion.AngleAxis(angle, Vector3.up), targetContainer.transform);

				// Should spawn weapon too?
				CharacterController character = newObj.GetComponentInChildren<CharacterController>();
				if (character != null)
				{
					foreach (GameObject weapon in GetDefaultWeapons())
						Instantiate(weapon, spawnPosition + Vector3.up * 1.5f, Quaternion.identity, targetContainer.transform);

					AIController aiController = newObj.GetComponentInChildren<AIController>();
					if(!aiController.HasDefaultProfile)
						aiController.SetProfile(GetAIProfile());
				}
			}
		}
	}

	private AIProfile GetAIProfile()
	{
		return m_FloorSettings.m_AIProfile;
	}

	private GameObject[] GetDefaultWeapons()
	{
		return m_FloorSettings.m_DefaultWeapons;
	}

	private PlacementSettings GetEnemyPlacementSettings()
	{
		return m_RoomSettings.m_OverrideFloorEnemyPlacement ? m_RoomSettings.m_EnemyPlacements : m_FloorSettings.m_EnemyPlacements;
	}

	private PlacementSettings GetLootPlacementSettings()
	{
		return m_RoomSettings.m_OverrideFloorLootPlacement ? m_RoomSettings.m_LootPlacements : m_FloorSettings.m_LootPlacements;
	}

	private PlacementSettings GetWallDressingSettings()
	{
		return m_RoomSettings.m_OverrideFloorWallDressingPlacement ? m_RoomSettings.m_WallDressingPlacements : m_FloorSettings.m_WallDressingPlacements;
	}

	private PlacementSettings GetFloorDressingSettings()
	{
		return m_RoomSettings.m_OverrideFloorFloorDressingPlacement ? m_RoomSettings.m_FloorDressingPlacements : m_FloorSettings.m_FloorDressingPlacements;
	}

	public static RoomInstance PlaceRoom(RoomInstance prefab, bool isBossRoom, FloorSettings floorSettings, RoomSettings roomSettings, Vector3Int location, Vector3Int extents, RoomConnections connection)
	{
		RoomInstance instance = Instantiate(prefab, location, Quaternion.identity);
		instance.m_Connections = connection;
		instance.m_IsBossRoom = isBossRoom;
		instance.m_RoomSettings = roomSettings;
		instance.m_FloorSettings = floorSettings;

		RoomGeneratorHelper generator = new RoomGeneratorHelper(roomSettings, extents, connection);
		instance.SetupRoom(generator, extents);
		return instance;
	}

	public bool HasTopDoor
	{
		get { return (m_Connections & RoomConnections.Top) != 0; }
	}

	public bool HasBottomDoor
	{
		get { return (m_Connections & RoomConnections.Bottom) != 0; }
	}

	public bool HasLeftDoor
	{
		get { return (m_Connections & RoomConnections.Left) != 0; }
	}

	public bool HasRightDoor
	{
		get { return (m_Connections & RoomConnections.Right) != 0; }
	}
}
