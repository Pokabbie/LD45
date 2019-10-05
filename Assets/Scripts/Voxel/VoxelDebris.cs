using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelDebris : MonoBehaviour
{
	private Renderer m_Renderer;
	private Rigidbody m_Body;

	private float m_Lifetime;
	private float m_Tracker;
	private float m_Scale;

	void Start()
	{
		m_Renderer = GetComponent<Renderer>();
		m_Body = GetComponent<Rigidbody>();
	}
	
	void Update()
    {
		m_Tracker += Time.deltaTime;
		
		if (m_Tracker > m_Lifetime)
		{
			ObjectPooler.Main.ReturnObject(gameObject);
		}
		else
		{
			float normalizedTime = m_Tracker / m_Lifetime;
			float startAnim = 0.7f;

			// Animate shrink
			if (normalizedTime > startAnim)
			{
				float t = Mathf.Lerp(m_Scale, 0.0f, (normalizedTime - startAnim) / (1.0f - startAnim));
				transform.localScale = Vector3.one * t;
			}
		}
    }

	public void SetColour(Color colour)
	{
		if (m_Renderer == null)
			m_Renderer = GetComponent<Renderer>();

		m_Renderer.material.color = colour;
	}

	public void ApplyExplostion(float force, Vector3 position, Vector3 sprayDirection)
	{
		if (m_Body == null)
			m_Body = GetComponent<Rigidbody>();

		Vector3 dir = (transform.position - position).normalized;
		m_Body.AddForce(dir * force * UnityEngine.Random.Range(0.8f, 1.2f));
		m_Body.AddForce(sprayDirection.normalized * force);
	}

	public static VoxelDebris NewDebris(VoxelDebris source, VoxelObject sourceData, Voxel voxel, float lifeTime, Vector3 position, Quaternion rotation)
	{
		VoxelDebris debris = ObjectPooler.Main.GetObject(source.gameObject, position, rotation).GetComponent<VoxelDebris>();
		debris.m_Tracker = 0.0f;
		debris.m_Lifetime = lifeTime;
		debris.m_Scale = sourceData.m_Scale;
		debris.transform.localScale = Vector3.one * sourceData.m_Scale;

		if (debris.m_Body != null)
		{
			debris.m_Body.velocity = Vector3.zero;
		}

		return debris;
	}
}
