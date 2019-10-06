using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelDestructable : Damageable
{
	[SerializeField]
	private int m_Health = 1;

	[SerializeField]
	private int m_DebrisSkip = 1;
	
	public override void ApplyDamage(GameObject source)
	{
		base.ApplyDamage(source);

		if (--m_Health <= 0)
		{
			VoxelModel model = GetComponentInChildren<VoxelModel>();
			VoxelDebrisController.Main.SpawnDebris(model.m_VoxelData, transform, (transform.position - source.transform.position), m_DebrisSkip);
			Destroy(gameObject);
		}
	}
}
