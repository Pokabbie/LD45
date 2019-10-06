using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AIProfile : ScriptableObject
{
	[Header("Overall")]
	public float m_TickRate = 0.5f;

	[Header("Movement")]
	public float m_IdleMoveThreshold = 0.8f;
	public float m_IdleMinMoveDistance = 2.0f;
	public float m_IdleMaxMoveDistance = 3.0f;
	public float m_IdleMoveUrgency = 0.1f;
	public float m_MaxSpeedFactor = 1.0f;
	public float m_MoveSpeedFactor = 1.0f;

	[Header("Weapon Collecting")]
	public float m_WeaponSearchMaxDistance = 10.0f;
	public int m_MinumumWeaponCount = 1;
	public int m_DesiredWeaponCount = 2;

	[Header("Attacking")]
	public bool m_RequireLOS = false;
	public float m_ShootCooldownScale = 1.0f;
	public float m_StrafeThreshold = 0.7f;
	public float m_StrafeMinDistance = 3.0f;
	public float m_StrafeMaxDistance = 5.0f;

	[Header("Agro")]
	public float m_SpotRange = 10.0f;
	public float m_WeaponAttackRangeConsideration = 0.1f;
	public float m_MaxWeaponAttackRangeInCalc = 10.0f;
	public float m_MinAttackRangeOffset = 5.0f;
	public float m_MaxAttackRangeOffset = 10.0f;

	[Header("Fleeing")]
	public float m_MinFleeRange = 5.0f;
	public float m_MaxFleeRange = 15.0f;
}
