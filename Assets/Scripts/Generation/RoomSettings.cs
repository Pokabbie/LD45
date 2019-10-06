using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RoomSettings : ScriptableObject
{
	[Header("General")]
	public Vector3Int m_Padding = new Vector3Int(10, 10, 10);

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
}
