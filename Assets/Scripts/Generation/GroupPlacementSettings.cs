using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GroupPlacementSettings : ScriptableObject
{
	public int m_MinElements = 0;
	public int m_MaxElements = 0;
	public ObjectPlacementSettings[] m_Objects;
	public GameObject[] m_AlwaysPlacedObjects;

	public ObjectPlacementSettings SelectRandomObject()
	{
		float totalWeight = 0.0f;
		for (int i = 0; i < m_Objects.Length; ++i)
			totalWeight += m_Objects[i].m_Weight;

		float weight = UnityEngine.Random.Range(0.0f, totalWeight);

		for (int i = 0; i < m_Objects.Length - 1; ++i)
		{
			weight -= m_Objects[i].m_Weight;
			if (weight <= 0.0f)
				return m_Objects[i];
		}

		return m_Objects[m_Objects.Length - 1];
	}
}