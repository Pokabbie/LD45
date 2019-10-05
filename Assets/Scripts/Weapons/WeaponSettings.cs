using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WeaponSettings : ScriptableObject
{
	public bool m_Automatic = false;
	public float m_Cooldown;
}