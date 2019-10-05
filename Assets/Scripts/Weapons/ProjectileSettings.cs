using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ProjectileSettings : ScriptableObject
{
	public float m_LifeTime = 3.0f;
	public int m_BounceCount = 0;
	public float m_Speed = 1.0f;
}
