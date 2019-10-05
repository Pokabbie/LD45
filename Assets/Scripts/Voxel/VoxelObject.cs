using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelObject : ScriptableObject
{
	public float m_Scale = 1.0f;
	public GameObject m_ModelObject;
	public VoxelData[] m_VoxelData;
}
