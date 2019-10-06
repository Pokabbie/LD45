using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GroupPlacementPlacement
{
	public float m_Weight = 1.0f;
	public GroupPlacementSettings m_PlacedGroup;
}

[Serializable]
public class PlacementSettings
{
	public int m_MinPlacements = 0;
	public int m_MaxPlacements = 0;
	public GroupPlacementPlacement[] m_Groups;

	public GroupPlacementSettings SelectRandomGroup()
	{
		float totalWeight = 0.0f;
		for (int i = 0; i < m_Groups.Length; ++i)
			totalWeight += m_Groups[i].m_Weight;

		float weight = UnityEngine.Random.Range(0.0f, totalWeight);

		for (int i = 0; i < m_Groups.Length - 1; ++i)
		{
			weight -= m_Groups[i].m_Weight;
			if (weight <= 0.0f)
				return m_Groups[i].m_PlacedGroup;
		}

		return m_Groups[m_Groups.Length - 1].m_PlacedGroup;
	}
}