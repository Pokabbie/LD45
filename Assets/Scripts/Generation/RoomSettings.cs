using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RoomSettings : ScriptableObject
{
	[Header("General")]
	public Vector3Int m_MaxBounds = new Vector3Int(20, 20, 20);
	public Vector3Int m_MinBounds = new Vector3Int(100, 30, 100);
	public Vector3Int m_Padding = new Vector3Int(10, 10, 10);

	public int m_FloorHeight = 0;
	public int m_CorridorHalfSize = 1;
	public float m_Scale = 0.5f;

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
