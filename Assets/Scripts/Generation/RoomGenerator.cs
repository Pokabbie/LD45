using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class RoomGenerator : MonoBehaviour
{
	public RoomSettings TEMPSETTINGS;

	[SerializeField]
	private GameObject m_RoomPrefab;

	// Start is called before the first frame update
	void Start()
    {
		Generator(TEMPSETTINGS, RoomConnections.Top | RoomConnections.Left);
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	public GameObject Generator(RoomSettings settings, RoomConnections connection)
	{
		GameObject obj = Instantiate(m_RoomPrefab);
		//obj.SetActive(false);
		obj.transform.parent = transform;

		RoomInstance instance = new RoomInstance(settings, connection);
		obj.GetComponent<MeshFilter>().mesh = instance.GeneratedMesh;
		obj.GetComponent<MeshCollider>().sharedMesh = instance.GeneratedMesh;
		return obj;
	}
}

internal class RoomInstance
{
	private Vector3Int m_TraversableExtent;
	private Mesh m_Mesh;

	private RoomSettings m_Settings;
	private RoomConnections m_Connections;
	private PerlinNoise m_NoiseMap;
	private Vector2Int m_DoorCentre;

	public RoomInstance(RoomSettings settings, RoomConnections connections)
	{
		m_Settings = settings;
		m_Connections = connections;
		m_NoiseMap = new PerlinNoise(Random.Range(int.MinValue, int.MaxValue));

		// Decide room size
		m_TraversableExtent = new Vector3Int(
			Random.Range(settings.m_MinBounds.x, settings.m_MaxBounds.x),
			Random.Range(settings.m_MinBounds.y, settings.m_MaxBounds.y),
			Random.Range(settings.m_MinBounds.z, settings.m_MaxBounds.z)
		);

		// Populate voxel data
		Vector3Int fullExtent = FullExtent;
		VoxelData voxelData = VoxelData.New(fullExtent.x, fullExtent.y, fullExtent.z);
		m_DoorCentre = new Vector2Int(fullExtent.x / 2, fullExtent.z / 2);

		for (int x = 0; x < fullExtent.x; ++x)
			for (int z = 0; z < fullExtent.z; ++z)
			{
				int height = GetHeight(x, z);

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

	private float GetRawNoise(int x, int z, float freq, float scale)
	{
		return (m_NoiseMap.GetNoise(x, z, freq, scale) + scale) * 0.5f;
	}

	private float GetProcessedNoise(int x, int z)
	{
		float noise = GetRawNoise(x, z, m_Settings.m_HeightFrequency, m_Settings.m_NoiseScale);
		if (noise > m_Settings.m_NoiseThreshold)
		{
			return 1.0f;// (noise - m_Settings.m_NoiseThreshold) / (m_Settings.m_NoiseScale - m_Settings.m_NoiseThreshold);
		}

		return 0.0f;
	}

	private int GetHeight(int x, int z)
	{
		// Ensure there is always a path from one room to another
		if (InCorridor(x, z))
			return m_Settings.m_FloorHeight;

		float noise = GetProcessedNoise(x, z);
		int height = Mathf.Max(m_Settings.m_FloorHeight, (int)(m_TraversableExtent.y * noise));

		if (InWalledArea(x, z))
			height += m_Settings.m_Padding.y;

		return height;
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
	
	private bool InWalledArea(int x, int z)
	{
		return MinDistanceFromWall(x, z) <= 0;
	}

	private bool InCorridor(int x, int z)
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
}