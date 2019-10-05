using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System;
using System.IO;
using System.Linq;

// Work around import splitting, just look for .vox files with same file name
[ScriptedImporter(1, "voximport")]
public class VoxImporter : ScriptedImporter
{
	public Material m_SourceMaterial;
	public float m_Scale = 0.1f;
	public Vector3 m_PivotOffset;

	public override void OnImportAsset(AssetImportContext ctx)
	{
		int counter = 0;
		
		// Main import object
		VoxelObject importObject = ScriptableObject.CreateInstance<VoxelObject>();
		importObject.m_Scale = m_Scale;
		importObject.name = Path.GetFileNameWithoutExtension(ctx.assetPath);

		ctx.AddObjectToAsset(importObject.name, importObject);
		ctx.SetMainObject(importObject);

		// Main game import object
		GameObject rootObject = new GameObject();
		rootObject.name = importObject.name;
		
		ctx.AddObjectToAsset(rootObject.name, rootObject);
		importObject.m_ModelObject = rootObject;

		List<VoxelData> allData = new List<VoxelData>();

		foreach (string voxFile in Directory.EnumerateFiles(Path.GetDirectoryName(ctx.assetPath), importObject.name + "*.vox"))
		{
			ctx.DependsOnSourceAsset(voxFile);
			
			// Add all separate data with model and voxel data
			Color32[] palette;
			foreach (VoxelData data in VoxelData.ParseFrom(voxFile, out palette))
			{
				allData.Add(data);

				data.name = "VOX_" + importObject.name + "_" + counter;
				ctx.AddObjectToAsset(data.name, data);

				Mesh mesh = data.GenerateMesh(m_PivotOffset, m_Scale);
				mesh.name = "MDL_" + importObject.name + "_" + counter;
				ctx.AddObjectToAsset(mesh.name, mesh);

				// Setup a game object to hold this
				GameObject modelObject = new GameObject();
				modelObject.name = "OBJ_" + importObject.name + "_" + counter;

				MeshFilter meshFilter = modelObject.AddComponent<MeshFilter>();
				MeshRenderer renderer = modelObject.AddComponent<MeshRenderer>();

				meshFilter.mesh = mesh;
				renderer.material = m_SourceMaterial;

				modelObject.transform.SetParent(rootObject.transform);
				ctx.AddObjectToAsset(modelObject.name, modelObject);

				++counter;
			}
		}

		importObject.m_VoxelData = allData.ToArray();
	}

	public Texture2D GetPalette(Color32[] palette)
	{
		Texture2D tex = new Texture2D(palette.Length, 1);
		tex.SetPixels32(palette);
		return tex;
	}
}

internal class VoxReader : IDisposable
{
	public struct VoxPoint
	{
		public byte x, y, z;
		public byte colourIndex;
	}

	public class VoxModel
	{
		public int m_SizeX;
		public int m_SizeY;
		public int m_SizeZ;
		public VoxPoint[] m_Voxels;
	}

	private string m_Path;
	private BinaryReader m_Reader;

	public int m_VersionNumber;
	public int m_ModelCount = 1; // Default is always one
	public uint[] m_Palette;
	public List<VoxModel> m_Models;

	private VoxModel m_CurrentModel;

	public VoxReader(string path)
	{
		m_Path = path;
		m_Reader = new BinaryReader(new FileStream(path, FileMode.Open));
		m_Models = new List<VoxModel>();
	}

	public void Dispose()
	{
		m_Reader.Dispose();
	}

	public Color32 GetColour(int index)
	{
		uint rawColour = m_Palette[index];
		byte[] bytes = BitConverter.GetBytes(rawColour);
		return new Color32(bytes[0], bytes[1], bytes[2], bytes[3]);
	}

	public void Read()
	{
		// Check header
		if (m_Reader.ReadByte() != 'V' || m_Reader.ReadByte() != 'O' || m_Reader.ReadByte() != 'X' || m_Reader.ReadByte() != ' ')
			throw new FormatException("Unable to find VOX at start of stream '" + m_Path + "'");

		m_VersionNumber = m_Reader.ReadInt32();
		ReadNextChunk();

		if (m_Palette == null)
			m_Palette = s_Defaultpalette;
	}

	public void ReadChunkDetails(out string chunkId, out int chunkByteCount, out int childrenChunkByteCount)
	{
		chunkId = "" + (char)m_Reader.ReadByte() + (char)m_Reader.ReadByte() + (char)m_Reader.ReadByte() + (char)m_Reader.ReadByte();
		chunkByteCount = m_Reader.ReadInt32();
		childrenChunkByteCount = m_Reader.ReadInt32();
	}

	public void ReadNextChunk()
	{
		string chunkId;
		int chunkByteCount, childrenByteCount;

		ReadChunkDetails(out chunkId, out chunkByteCount, out childrenByteCount);
		ReadChunk(chunkId, chunkByteCount);
	}

	public void ReadChunk(string chunkId, int byteCount)
	{
		switch (chunkId)
		{
			case "MAIN":
				ReadChunk_MAIN(byteCount);
				break;

			case "PACK":
				ReadChunk_PACK(byteCount);
				break;

			case "SIZE":
				ReadChunk_SIZE(byteCount);
				break;

			case "XYZI":
				ReadChunk_XYZI(byteCount);
				break;

			case "RGBA":
				ReadChunk_RGBA(byteCount);
				break;

			default:
				throw new FormatException("Unrecognised chunk id '" + chunkId + "' in '" + m_Path + "'");
		}
	}

	public void ReadChunk_MAIN(int byteCount)
	{
		while (m_Reader.PeekChar() != -1)
		{
			ReadNextChunk();
		}
	}

	public void ReadChunk_PACK(int byteCount)
	{
		m_ModelCount = m_Reader.ReadInt32();
	}

	public void ReadChunk_SIZE(int byteCount)
	{
		m_CurrentModel = new VoxModel();
		m_CurrentModel.m_SizeX = m_Reader.ReadInt32();
		m_CurrentModel.m_SizeY = m_Reader.ReadInt32();
		m_CurrentModel.m_SizeZ = m_Reader.ReadInt32();
	}

	public void ReadChunk_XYZI(int byteCount)
	{
		int numVoxels = m_Reader.ReadInt32();
		m_CurrentModel.m_Voxels = new VoxPoint[numVoxels];

		for (int i = 0; i < numVoxels; ++i)
		{
			VoxPoint point = new VoxPoint();
			point.x = m_Reader.ReadByte();
			point.y = m_Reader.ReadByte();
			point.z = m_Reader.ReadByte();
			point.colourIndex = m_Reader.ReadByte();
			m_CurrentModel.m_Voxels[i] = point;
		}

		m_Models.Add(m_CurrentModel);
		m_CurrentModel = null;
	}

	public void ReadChunk_RGBA(int byteCount)
	{
		m_Palette = new uint[256];

		for (int i = 0; i < 256; ++i)
		{
			m_Palette[i] = m_Reader.ReadUInt32();
		}
	}

	private uint[] s_Defaultpalette = new uint[256] {
		0x00000000, 0xffffffff, 0xffccffff, 0xff99ffff, 0xff66ffff, 0xff33ffff, 0xff00ffff, 0xffffccff, 0xffccccff, 0xff99ccff, 0xff66ccff, 0xff33ccff, 0xff00ccff, 0xffff99ff, 0xffcc99ff, 0xff9999ff,
		0xff6699ff, 0xff3399ff, 0xff0099ff, 0xffff66ff, 0xffcc66ff, 0xff9966ff, 0xff6666ff, 0xff3366ff, 0xff0066ff, 0xffff33ff, 0xffcc33ff, 0xff9933ff, 0xff6633ff, 0xff3333ff, 0xff0033ff, 0xffff00ff,
		0xffcc00ff, 0xff9900ff, 0xff6600ff, 0xff3300ff, 0xff0000ff, 0xffffffcc, 0xffccffcc, 0xff99ffcc, 0xff66ffcc, 0xff33ffcc, 0xff00ffcc, 0xffffcccc, 0xffcccccc, 0xff99cccc, 0xff66cccc, 0xff33cccc,
		0xff00cccc, 0xffff99cc, 0xffcc99cc, 0xff9999cc, 0xff6699cc, 0xff3399cc, 0xff0099cc, 0xffff66cc, 0xffcc66cc, 0xff9966cc, 0xff6666cc, 0xff3366cc, 0xff0066cc, 0xffff33cc, 0xffcc33cc, 0xff9933cc,
		0xff6633cc, 0xff3333cc, 0xff0033cc, 0xffff00cc, 0xffcc00cc, 0xff9900cc, 0xff6600cc, 0xff3300cc, 0xff0000cc, 0xffffff99, 0xffccff99, 0xff99ff99, 0xff66ff99, 0xff33ff99, 0xff00ff99, 0xffffcc99,
		0xffcccc99, 0xff99cc99, 0xff66cc99, 0xff33cc99, 0xff00cc99, 0xffff9999, 0xffcc9999, 0xff999999, 0xff669999, 0xff339999, 0xff009999, 0xffff6699, 0xffcc6699, 0xff996699, 0xff666699, 0xff336699,
		0xff006699, 0xffff3399, 0xffcc3399, 0xff993399, 0xff663399, 0xff333399, 0xff003399, 0xffff0099, 0xffcc0099, 0xff990099, 0xff660099, 0xff330099, 0xff000099, 0xffffff66, 0xffccff66, 0xff99ff66,
		0xff66ff66, 0xff33ff66, 0xff00ff66, 0xffffcc66, 0xffcccc66, 0xff99cc66, 0xff66cc66, 0xff33cc66, 0xff00cc66, 0xffff9966, 0xffcc9966, 0xff999966, 0xff669966, 0xff339966, 0xff009966, 0xffff6666,
		0xffcc6666, 0xff996666, 0xff666666, 0xff336666, 0xff006666, 0xffff3366, 0xffcc3366, 0xff993366, 0xff663366, 0xff333366, 0xff003366, 0xffff0066, 0xffcc0066, 0xff990066, 0xff660066, 0xff330066,
		0xff000066, 0xffffff33, 0xffccff33, 0xff99ff33, 0xff66ff33, 0xff33ff33, 0xff00ff33, 0xffffcc33, 0xffcccc33, 0xff99cc33, 0xff66cc33, 0xff33cc33, 0xff00cc33, 0xffff9933, 0xffcc9933, 0xff999933,
		0xff669933, 0xff339933, 0xff009933, 0xffff6633, 0xffcc6633, 0xff996633, 0xff666633, 0xff336633, 0xff006633, 0xffff3333, 0xffcc3333, 0xff993333, 0xff663333, 0xff333333, 0xff003333, 0xffff0033,
		0xffcc0033, 0xff990033, 0xff660033, 0xff330033, 0xff000033, 0xffffff00, 0xffccff00, 0xff99ff00, 0xff66ff00, 0xff33ff00, 0xff00ff00, 0xffffcc00, 0xffcccc00, 0xff99cc00, 0xff66cc00, 0xff33cc00,
		0xff00cc00, 0xffff9900, 0xffcc9900, 0xff999900, 0xff669900, 0xff339900, 0xff009900, 0xffff6600, 0xffcc6600, 0xff996600, 0xff666600, 0xff336600, 0xff006600, 0xffff3300, 0xffcc3300, 0xff993300,
		0xff663300, 0xff333300, 0xff003300, 0xffff0000, 0xffcc0000, 0xff990000, 0xff660000, 0xff330000, 0xff0000ee, 0xff0000dd, 0xff0000bb, 0xff0000aa, 0xff000088, 0xff000077, 0xff000055, 0xff000044,
		0xff000022, 0xff000011, 0xff00ee00, 0xff00dd00, 0xff00bb00, 0xff00aa00, 0xff008800, 0xff007700, 0xff005500, 0xff004400, 0xff002200, 0xff001100, 0xffee0000, 0xffdd0000, 0xffbb0000, 0xffaa0000,
		0xff880000, 0xff770000, 0xff550000, 0xff440000, 0xff220000, 0xff110000, 0xffeeeeee, 0xffdddddd, 0xffbbbbbb, 0xffaaaaaa, 0xff888888, 0xff777777, 0xff555555, 0xff444444, 0xff222222, 0xff111111
	};
}
