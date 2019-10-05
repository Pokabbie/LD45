using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Custom parser only supporting Magic Voxel
[Serializable]
public class VoxelData : ScriptableObject
{
	[SerializeField]
	public Voxel[] m_RawData;

	[SerializeField]
	private int m_Width;

	[SerializeField]
	private int m_Height;

	[SerializeField]
	private int m_Depth;

	[SerializeField]
	private Vector3 m_Centre;

	// Format refer to
	// https://github.com/ephtracy/voxel-model/blob/master/MagicaVoxel-file-format-vox.txt
	public static IEnumerable<VoxelData> ParseFrom(string path, out Color32[] palette)
	{
		List<VoxelData> outputData = new List<VoxelData>();

		using (VoxReader reader = new VoxReader(path))
		{
			reader.Read();

			foreach (var model in reader.m_Models)
			{
				VoxelData data = CreateInstance<VoxelData>();
				data.m_Width = model.m_SizeX;
				data.m_Height = model.m_SizeY;
				data.m_Depth = model.m_SizeZ;
				data.m_RawData = new Voxel[model.m_SizeX * model.m_SizeY * model.m_SizeZ];

				foreach (var voxel in model.m_Voxels)
				{
					if (voxel.colourIndex == 0)
						Debug.LogWarning("Colour Index 0 found!");

					data.m_RawData[data.GetRawIndex(voxel.x, voxel.y, voxel.z)] = new Voxel(voxel.colourIndex);
				}

				outputData.Add(data);
			}

			palette = new Color32[reader.m_Palette.Length];
			for (int i = 0; i < palette.Length; ++i)
				palette[i] = reader.GetColour(i);
		}
		
		return outputData;
	}

	private int GetRawIndex(int x, int y, int z)
	{
		return z * m_Height * m_Width + y * m_Width + x;
	}

	public int Width
	{
		get { return m_Width; }
	}

	public int Height
	{
		get { return m_Height; }
	}

	public int Depth
	{
		get { return m_Depth; }
	}

	public Vector3 Centre
	{
		get { return m_Centre; }
	}
	
	public Voxel GetVoxel(int x, int y, int z)
	{
		if (x < 0 || x >= m_Width
		|| y < 0 || y >= m_Height
		|| z < 0 || z >= m_Depth
			)
		{
			return Voxel.Empty;
		}

		return m_RawData[GetRawIndex(x, y, z)];
	}

	internal Mesh GenerateMesh(Vector3 pivotOffset, float scale)
	{
		m_Centre = pivotOffset + new Vector3(Width, Height, Depth) * 0.5f;
		VoxelMeshGenerator generator = new VoxelMeshGenerator(this, scale);
		return generator.GenerateMesh();
	}

	public Vector3 GetVoxelPosition(int x, int y, int z, float scale)
	{
		return (new Vector3(x,y,z) - m_Centre) * scale;
	}
}

internal class VoxelMeshGenerator
{
	private VoxelData m_Data;
	private Vector3 m_Centre;
	private float m_Scale;

	private List<Vector3> m_Positions = new List<Vector3>();
	private List<Vector3> m_Normals = new List<Vector3>();
	private List<Vector2> m_UVs = new List<Vector2>();
	private List<int> m_Indices = new List<int>();

	public VoxelMeshGenerator(VoxelData data, float scale)
	{
		m_Data = data;
		m_Scale = scale;
		m_Centre = data.Centre;
	}

	public Mesh GenerateMesh()
	{
		for (int x = 0; x < m_Data.Width; ++x)
			for (int y = 0; y < m_Data.Height; ++y)
				for (int z = 0; z < m_Data.Depth; ++z)
				{
					TryPutFace(x, y, z, 0, 0, 1);
					TryPutFace(x, y, z, 0, 0, -1);
					TryPutFace(x, y, z, 0, 1, 0);
					TryPutFace(x, y, z, 0, -1, 0);
					TryPutFace(x, y, z, 1, 0, 0);
					TryPutFace(x, y, z, -1, 0, 0);
				}

		Mesh mesh = new Mesh();
		mesh.SetVertices(m_Positions);
		mesh.SetNormals(m_Normals);
		mesh.SetUVs(0, m_UVs);
		mesh.SetIndices(m_Indices.ToArray(), MeshTopology.Triangles, 0);
		return mesh;
	}

	private void TryPutFace(int x, int y, int z, int dx, int dy, int dz)
	{
		if (ShouldPutFace(x, y, z, dx, dy, dz))
			PutFace(x, y, z, dx, dy, dz);
	}

	private bool ShouldPutFace(int x, int y, int z, int dx, int dy, int dz)
	{
		if (!m_Data.GetVoxel(x, y, z).IsEmpty)
		{
			return m_Data.GetVoxel(x + dx, y + dy, z + dz).IsEmpty;
		}
		return false;
	}

	private void PutFace(int x, int y, int z, int dx, int dy, int dz)
	{
		uint colourIndex = m_Data.GetVoxel(x, y, z).m_ColourIndex;
		Vector2 uv = new Vector2(((colourIndex - 1) + 0.5f) / 256.0f, 0.5f);
		m_UVs.Add(uv);
		m_UVs.Add(uv);
		m_UVs.Add(uv);
		m_UVs.Add(uv);

		int sign = 0;
		int i0 = -1;
		int i1 = -1;
		int i2 = -1;
		int i3 = -1;

		// Add left/right
		if (dx != 0)
		{
			sign = dx >= 0 ? 1 : -1;
			i0 = AddVertex(x, y, z,  1 * sign,  1,  1);
			i1 = AddVertex(x, y, z,  1 * sign,  1, -1);
			i2 = AddVertex(x, y, z,  1 * sign, -1,  1);
			i3 = AddVertex(x, y, z,  1 * sign, -1, -1);
		}

		// Add top/bottom
		if (dy != 0)
		{
			sign = dy >= 0 ? 1 : -1;
			i0 = AddVertex(x, y, z,  1,  1 * sign,  1);
			i1 = AddVertex(x, y, z, -1,  1 * sign,  1);
			i2 = AddVertex(x, y, z,  1,  1 * sign, -1);
			i3 = AddVertex(x, y, z, -1,  1 * sign, -1);
		}

		// Add front/back
		else if (dz != 0)
		{
			sign = dz >= 0 ? 1 : -1;
			i0 = AddVertex(x, y, z, -1,  1,  1 * sign);
			i1 = AddVertex(x, y, z,  1,  1,  1 * sign);
			i2 = AddVertex(x, y, z, -1, -1,  1 * sign);
			i3 = AddVertex(x, y, z,  1, -1,  1 * sign);
		}

		Vector3 normal = new Vector3(dx, dy, dz);
		m_Normals.Add(normal);
		m_Normals.Add(normal);
		m_Normals.Add(normal);
		m_Normals.Add(normal);

		if (sign == 1)
		{
			m_Indices.Add(i0);
			m_Indices.Add(i2);
			m_Indices.Add(i1);

			m_Indices.Add(i2);
			m_Indices.Add(i3);
			m_Indices.Add(i1);
		}
		else
		{
			m_Indices.Add(i0);
			m_Indices.Add(i1);
			m_Indices.Add(i2);

			m_Indices.Add(i2);
			m_Indices.Add(i1);
			m_Indices.Add(i3);
		}
	}

	private Vector3 GetPoint(int x, int y, int z, int dx, int dy, int dz)
	{
		return (new Vector3(
			(x + (dx * 0.5f)),
			(y + (dy * 0.5f)),
			(z + (dz * 0.5f))
		) - m_Centre) * m_Scale;
	}

	private int AddVertex(int x, int y, int z, int dx, int dy, int dz)
	{
		m_Positions.Add(GetPoint(x, y, z, dx, dy, dz));
		return m_Positions.Count - 1;
	}
}