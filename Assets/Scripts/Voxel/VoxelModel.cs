using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VoxelModel : MonoBehaviour
{
	public VoxelObject m_VoxelData;
	private VoxelObject m_PreviousVoxelData;
	
	void Awake()
    {
		if (m_VoxelData != m_PreviousVoxelData)
		{
			m_PreviousVoxelData = m_VoxelData;

			for (int i = 0; i < transform.childCount; ++i)
			{
				DestroyImmediate(transform.GetChild(i).gameObject);
			}
			
			if (m_VoxelData != null && m_VoxelData.m_ModelObject != null)
			{
				GameObject gameObj = Instantiate(m_VoxelData.m_ModelObject, transform);
				HideFlags flags = HideFlags.DontSave | HideFlags.NotEditable | HideFlags.HideInInspector;
				UpdateFlags(gameObj.transform, flags);
			}
		}
	}

	private static void UpdateFlags(Transform transform, HideFlags flags)
	{
		transform.gameObject.hideFlags = flags;
		for (int i = 0; i < transform.childCount; ++i)
		{
			UpdateFlags(transform.GetChild(i), flags);
		}
	}
}
