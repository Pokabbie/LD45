using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Flags]
public enum RoomConnections
{
	None	= 0,
	Top		= 1,
	Bottom	= 2,
	Left	= 4,
	Right	= 8,

	All = Top | Bottom | Left | Right
}

// TEMP REMOVE THIS LATER
public class RoomGenerator : MonoBehaviour
{
	public RoomSettings TEMPSETTINGS;
	public Vector3Int TEMPEXTENT;

	[SerializeField]
	private RoomInstance m_RoomPrefab;

	// Start is called before the first frame update
	void Start()
	{
		RoomInstance.PlaceRoom(m_RoomPrefab, TEMPSETTINGS, new Vector3Int(0, 0, 0), new Vector3Int(50, 3, 50), RoomConnections.Top | RoomConnections.Left);
		//RoomInstance.PlaceRoom(m_RoomPrefab, TEMPSETTINGS, new Vector3Int(-50, 0, 0), new Vector3Int(50, 3, 50), RoomConnections.Top | RoomConnections.Right);

		//RoomInstance.PlaceRoom(m_RoomPrefab, TEMPSETTINGS, new Vector3Int(0, 0, 75), new Vector3Int(50, 3, 100), RoomConnections.Top | RoomConnections.Bottom);
		//RoomInstance.PlaceRoom(m_RoomPrefab, TEMPSETTINGS, new Vector3Int(0, 0, 150), new Vector3Int(50, 3, 50), RoomConnections.Bottom);
	}

    // Update is called once per frame
    void Update()
    {
	}
}

internal class RoomGeneratorHelper
{
	private Vector3Int m_TraversableExtent;
	private Mesh m_Mesh;

	private RoomSettings m_Settings;
	private RoomConnections m_Connections;
	private PerlinNoise m_NoiseMap;
	private Vector2Int m_DoorCentre;

	private List<Vector3Int> m_AccessibleSpots = new List<Vector3Int>();
	private List<Vector3Int> m_InaccessibleSpots = new List<Vector3Int>();

	public RoomGeneratorHelper(RoomSettings settings, Vector3Int targetExtent, RoomConnections connections)
	{
		m_Settings = settings;
		m_Connections = connections;
		m_NoiseMap = new PerlinNoise(Random.Range(int.MinValue, int.MaxValue));

		// Decide room size
		m_TraversableExtent = new Vector3Int(
			targetExtent.x - settings.m_Padding.x * 2,
			targetExtent.y,
			targetExtent.z - settings.m_Padding.z * 2
		);

		// Populate voxel data
		Vector3Int fullExtent = FullExtent;
		VoxelData voxelData = VoxelData.New(fullExtent.x, fullExtent.y, fullExtent.z);
		m_DoorCentre = new Vector2Int(fullExtent.x / 2, fullExtent.z / 2);

		for (int x = 0; x < fullExtent.x; ++x)
			for (int z = 0; z < fullExtent.z; ++z)
			{
				int height = GetHeight(x, z);

				// Try to create a placement spot here
				if (x % m_Settings.m_PlacementFrequency == 0 && z % m_Settings.m_PlacementFrequency == 0)
				{
					if (height == m_Settings.m_FloorHeight)
						m_AccessibleSpots.Add(new Vector3Int(x, height, z));
					else if (height == m_TraversableExtent.y)
						m_InaccessibleSpots.Add(new Vector3Int(x, height, z));
				}


				for (int y = 0; y <= height; ++y)
				{
					int ry = (height - y);

					uint[] coloursTable = m_Settings.m_ColourIndices;

					float rawNoise = GetRawNoise(x, z, m_Settings.m_TextureHeightFrequency, m_Settings.m_TextureNoiseScale);

					if (rawNoise <= 0.3f && m_Settings.m_LowNoiseColourIndices.Length != 0)
						coloursTable = m_Settings.m_LowNoiseColourIndices;
					else if (rawNoise >= 0.6f && m_Settings.m_HighNoiseColourIndices.Length != 0)
						coloursTable = m_Settings.m_HighNoiseColourIndices;
					
					int colourLookupIdx = Mathf.Clamp(ry, 0, coloursTable.Length - 1);
					uint colour = coloursTable[colourLookupIdx];
					
					voxelData.SetVoxel(new Voxel(colour), x, y, z);
				}
			}
		
		m_Mesh = voxelData.GenerateMesh(Vector3.zero, settings.m_Scale);
		m_Mesh.name = "GeneratedMesh";
	}

	public Vector3Int TravesableExtent
	{
		get { return m_TraversableExtent; }
	}

	public Vector3Int FullExtent
	{
		get { return TravesableExtent + m_Settings.m_Padding * 2; }
	}

	public Mesh GeneratedMesh
	{
		get { return m_Mesh; }
	}

	public IEnumerable<Vector3Int> AccessibleSpots
	{
		get { return m_AccessibleSpots; }
	}

	public IEnumerable<Vector3Int> InaccessibleSpots
	{
		get { return m_InaccessibleSpots; }
	}

	public float GetRawNoise(int x, int z, float freq, float scale)
	{
		return (m_NoiseMap.GetNoise(x, z, freq, scale) + scale) * 0.5f;
	}

	public float GetProcessedNoise(int x, int z)
	{
		float noise = GetRawNoise(x, z, m_Settings.m_HeightFrequency, m_Settings.m_NoiseScale);
		if (noise > m_Settings.m_NoiseThreshold)
		{
			return 1.0f;// (noise - m_Settings.m_NoiseThreshold) / (m_Settings.m_NoiseScale - m_Settings.m_NoiseThreshold);
		}

		return 0.0f;
	}

	public int GetHeight(int x, int z)
	{
		// Ensure there is always a path from one room to another
		if (InCorridor(x, z) || DistanceFromCentreSq(x, z) <= m_Settings.m_CentreRadius * m_Settings.m_CentreRadius)
			return m_Settings.m_FloorHeight;

		if (InWall(x, z))
			return m_TraversableExtent.y;

		float noise = GetProcessedNoise(x, z);
		int height = Mathf.Max(m_Settings.m_FloorHeight, (int)(m_TraversableExtent.y * noise));
		
		return height;
	}

	public IEnumerable<Vector3Int> GetFreeAccessibleSpot(int count)
	{
		return GetFreeSpot(count, m_AccessibleSpots);
	}

	public IEnumerable<Vector3Int> GetFreeInaccessibleSpot(int count)
	{
		return GetFreeSpot(count, m_InaccessibleSpots);
	}

	private IEnumerable<Vector3Int> GetFreeSpot(int count, List<Vector3Int> targetList)
	{
		if (count <= 0 || targetList.Count == 0)
			return new Vector3Int[0];

		int randomIdx = Random.Range(0, targetList.Count - 1);
		List<Vector3Int> outputs = new List<Vector3Int>();

		Vector3Int startSpot = targetList[randomIdx];
		outputs.Add(startSpot);
		targetList.RemoveAt(randomIdx);

		if (count > 1)
		{
			IEnumerable<Vector3Int> sorted = targetList.OrderBy((spot) =>
			{
				float dx = (spot.x - startSpot.x);
				float dz = (spot.z - startSpot.z);
				return dx * dx + dz * dz;
			});
			var consumedSpots = sorted.Take(count - 1).ToArray();

			foreach (Vector3Int spot in consumedSpots)
			{
				outputs.Add(spot);
				targetList.Remove(spot);
			}
		}

		return outputs;
	}

	private float DistanceFromCentreSq(int x, int z)
	{
		float centreX = FullExtent.x / 2.0f;
		float centreZ = FullExtent.z / 2.0f;

		float dx = centreX - x;
		float dz = centreZ - z;
		return dx * dx + dz * dz;
	}

	private int MinDistanceFromWall(int x, int z)
	{
		int xDist, zDist;

		if (x < FullExtent.x / 2)
			xDist = x - m_Settings.m_Padding.x;
		else
			xDist = (m_Settings.m_Padding.x + m_TraversableExtent.x) - (x);

		if (z < FullExtent.z / 2)
			zDist = z - m_Settings.m_Padding.z;
		else
			zDist = (m_Settings.m_Padding.z + m_TraversableExtent.z) - (z);

		return Mathf.Min(xDist, zDist);
	}

	public bool InWall(int x, int z)
	{
		return MinDistanceFromWall(x, z) <= 0;
	}

	public bool InCorridor(int x, int z)
	{
		int doorMinX = m_DoorCentre.x - m_Settings.m_CorridorHalfSize;
		int doorMaxX = m_DoorCentre.x + m_Settings.m_CorridorHalfSize;

		int doorMinZ = m_DoorCentre.y - m_Settings.m_CorridorHalfSize;
		int doorMaxZ = m_DoorCentre.y + m_Settings.m_CorridorHalfSize;

		bool inXCorridor = (x >= doorMinX && x <= doorMaxX);
		bool inZCorridor = (z >= doorMinZ && z <= doorMaxZ);

		if ((m_Connections & RoomConnections.Top) != 0)
		{
			if (inXCorridor && z >= m_DoorCentre.y - m_Settings.m_CorridorHalfSize)
				return true;
		}
		if ((m_Connections & RoomConnections.Bottom) != 0)
		{
			if (inXCorridor && z <= m_DoorCentre.y + m_Settings.m_CorridorHalfSize)
				return true;
		}
		if ((m_Connections & RoomConnections.Right) != 0)
		{
			if (inZCorridor && x >= m_DoorCentre.x - m_Settings.m_CorridorHalfSize)
				return true;
		}
		if ((m_Connections & RoomConnections.Left) != 0)
		{
			if (inZCorridor && x <= m_DoorCentre.x + m_Settings.m_CorridorHalfSize)
				return true;
		}

		return false;
	}

	public bool GetDoor(RoomConnections door, out Vector2Int location)
	{
		if ((door & RoomConnections.Top) != 0)
		{
			location = new Vector2Int(m_DoorCentre.x, FullExtent.z - 1);
			return true;
		}
		if ((door & RoomConnections.Bottom) != 0)
		{
			location = new Vector2Int(m_DoorCentre.x, 0);
			return true;
		}
		if ((door & RoomConnections.Left) != 0)
		{
			location = new Vector2Int(0, m_DoorCentre.y);
			return true;
		}
		if ((door & RoomConnections.Right) != 0)
		{
			location = new Vector2Int(FullExtent.x - 1, m_DoorCentre.y);
			return true;
		}

		location = Vector2Int.zero;
		return false;
	}
	
	private bool CanSeeDoor(int x, int z)
	{
		Vector2Int location;
		if (GetDoor(RoomConnections.Top, out location) && CanSeePoint(x,z, location.x, location.y - m_Settings.m_Padding.z))
			return true;
		if (GetDoor(RoomConnections.Bottom, out location) && CanSeePoint(x, z, location.x, location.y + m_Settings.m_Padding.z))
			return true;
		if (GetDoor(RoomConnections.Left, out location) && CanSeePoint(x, z, location.x + m_Settings.m_Padding.x, location.y))
			return true;
		if (GetDoor(RoomConnections.Right, out location) && CanSeePoint(x, z, location.x - m_Settings.m_Padding.x, location.y))
			return true;

		return false;
	}

	public bool CanSeePoint(int x0, int z0, int x1, int z1)
	{
		if (x0 == x1 && z0 == z1)
			return true;

		// Line step check
		Vector2 baseLocation = new Vector2(x0, z0);
		Vector2 step = new Vector2(x1 - x0, z1 - z0).normalized;
		step /= Mathf.Max(Mathf.Abs(step.x), Mathf.Abs(step.y));

		int maxChecks = 100;// (FullExtent.x * FullExtent.z) / 2;
		for (int i = 1; i <= maxChecks; ++i)
		{
			Vector2 loc = baseLocation + step * i;

			int cX = (int)loc.x;
			int cZ = (int)loc.y;

			if (cX == x1 && cZ == z1)
				return true;

			if (cX < 0 || cX > FullExtent.x || cZ < 0 || cZ > FullExtent.z)
				break;

			int height = GetHeight(cX, cZ);
			if (height != m_Settings.m_FloorHeight)
				return false;
		}

		return true;
	}
}