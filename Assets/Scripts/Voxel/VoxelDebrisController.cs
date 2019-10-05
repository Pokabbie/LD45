using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelDebrisController : MonoBehaviour
{
	private static VoxelDebrisController s_Main;
	public static VoxelDebrisController Main
	{
		get { return s_Main; }
	}

	public VoxelDebris m_DebrisType;
	public float m_DebrisLifetime = 5.0f;

	[SerializeField]
	private Texture2D m_AtlasTexture;

	[SerializeField]
	private float m_ExplosiveForce = 1.0f;

	void Start()
    {
		if (s_Main == null)
			s_Main = this;
		else
			Debug.LogWarning("Multiple VoxelDebrisController found");
	}

	public void SpawnDebris(VoxelObject data, Vector3 position, Quaternion rotation, Vector3 sprayDirection)
	{
		foreach (var voxelData in data.m_VoxelData)
			SpawnDebris(data, voxelData, position, rotation, sprayDirection);
	}

	public void SpawnDebris(VoxelObject data, Transform source, Vector3 sprayDirection)
	{
		foreach (var voxelData in data.m_VoxelData)
			SpawnDebris(data, voxelData, source.position, source.rotation, sprayDirection);
	}

	private void SpawnDebris(VoxelObject sourceData, VoxelData data, Vector3 position, Quaternion rotation, Vector3 sprayDirection)
	{
		for (int x = 0; x < data.Width; ++x)
			for (int y = 0; y < data.Height; ++y)
				for (int z = 0; z < data.Depth; ++z)
				{
					Voxel voxel = data.GetVoxel(x, y, z);

					if (!voxel.IsEmpty)
					{
						Color colour = m_AtlasTexture.GetPixel((int)voxel.m_ColourIndex - 1, 0);

						VoxelDebris debris = VoxelDebris.NewDebris(m_DebrisType, sourceData, voxel, m_DebrisLifetime, position + rotation * data.GetVoxelPosition(x, y, z, sourceData.m_Scale), rotation);

						debris.SetColour(colour);
						debris.ApplyExplostion(m_ExplosiveForce, position, sprayDirection);
					}
				}
	}
}
