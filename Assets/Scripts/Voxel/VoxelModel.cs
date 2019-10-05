using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class VoxelModel : MonoBehaviour
{
	public VoxelObject m_VoxelData;
	private VoxelObject m_PreviousVoxelData;
	
	void Awake()
    {
		UpdateData();
	}

#if UNITY_EDITOR
	void Update()
	{
		UpdateData();
	}
#endif

	private void UpdateData()
	{
		if (m_VoxelData != m_PreviousVoxelData)
		{
			m_PreviousVoxelData = m_VoxelData;

			for (int i = 0; i < transform.childCount; ++i)
			{
				GameObject child = transform.GetChild(i).gameObject;
				if (child.CompareTag("Generated"))
					DestroyImmediate(transform.GetChild(i).gameObject);
			}

			if (m_VoxelData != null && m_VoxelData.m_ModelObject != null)
			{
				GameObject gameObj = Instantiate(m_VoxelData.m_ModelObject, transform);
				HideFlags flags = HideFlags.DontSave;

#if UNITY_EDITOR
				bool isPrefabSource = gameObject.scene.rootCount == 1;
				
				if (isPrefabSource)
				{
					flags = HideFlags.HideAndDontSave;
				}
#endif

				UpdateFlags(gameObj.transform, flags);
			}
		}
	}

	private static void UpdateFlags(Transform transform, HideFlags flags)
	{
		transform.gameObject.tag = "Generated";
		transform.gameObject.hideFlags = flags;
		for (int i = 0; i < transform.childCount; ++i)
		{
			UpdateFlags(transform.GetChild(i), flags);
		}
	}
}
