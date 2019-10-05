using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Voxel
{
	public uint m_ColourIndex;

	public bool IsEmpty
	{
		get { return m_ColourIndex == 0; }
	}

	public Voxel(uint colourIndex)
	{
		m_ColourIndex = colourIndex;
	}

	public static Voxel Empty
	{
		get { return new Voxel(); }
	}
}
