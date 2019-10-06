using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RoomSettings : ScriptableObject
{
	[Header("General")]
	public Vector3Int m_Padding = new Vector3Int(10, 10, 10);
	public Vector3Int m_MinExtents = new Vector3Int(20, 3, 20);
	public Vector3Int m_MaxExtents = new Vector3Int(20, 3, 20);

	public int m_FloorHeight = 0;
	public float m_CentreRadius = 10.0f;
	public int m_CorridorHalfSize = 1;
	public float m_Scale = 0.5f;

	[Header("Spawning")]
	public int m_PlacementFrequency = 3;

	public bool m_OverrideFloorEnemyPlacement = false;
	public PlacementSettings m_EnemyPlacements;
	public bool m_OverrideFloorLootPlacement = false;
	public PlacementSettings m_LootPlacements;

	public bool m_OverrideFloorWallDressingPlacement = false;
	public PlacementSettings m_WallDressingPlacements;
	public bool m_OverrideFloorFloorDressingPlacement = false;
	public PlacementSettings m_FloorDressingPlacements;

	[Header("Generation")]
	public float m_NoiseThreshold = 0.5f;
	public float m_NoiseScale = 1.0f;
	public float m_HeightFrequency = 1.0f;

	[Header("Texturing")]
	public float m_TextureNoiseScale = 1.0f;
	public float m_TextureHeightFrequency = 1.0f;

	public uint[] m_ColourIndices;
	public uint[] m_HighNoiseColourIndices;
	public uint[] m_LowNoiseColourIndices;

	public Vector3Int RandomExtents()
	{
		Vector3Int extents = new Vector3Int(
			UnityEngine.Random.Range(m_MinExtents.x, m_MaxExtents.x),
			UnityEngine.Random.Range(m_MinExtents.y, m_MaxExtents.y),
			UnityEngine.Random.Range(m_MinExtents.z, m_MaxExtents.z)
		);

		if (extents.x % 2 == 1)
			extents.x += 1;
		if (extents.y % 2 == 1)
			extents.y += 1;
		if (extents.z % 2 == 1)
			extents.z += 1;
		return extents;
	}
}
