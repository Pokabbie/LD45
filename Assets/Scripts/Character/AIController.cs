using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(CharacterController))]
public class AIController : MonoBehaviour
{
	public enum AIState
	{
		Idle,
		FindingWeapon,
		AttackingPlayer,
		Fleeing,
	}

	private CharacterController m_CharController;

	[SerializeField]
	private AIProfile m_DefaultProfile;

	private GameObject m_DiscoveredPlayer;
	private IEnumerable<GameObject> m_DiscoveredWeapons;

	private float m_DecisionTimer;
	private LayerMask m_LOSLayers;
	private AIProfile m_Profile;

	private AIState m_CurrentState;
	
	private float m_BehaviourNoise;
	private bool m_HasLineOfSight;

	private bool m_MoveToTarget;
	private float m_MoveUrgency;
	private Transform m_TargetMoveTransform;
	private Vector3 m_TargetMovePosition;

	private bool m_AimToTarget;
	private Transform m_TargetAimTransform;
	private Vector3 m_TargetAimPosition;

	void Start()
	{
		m_CharController = GetComponent<CharacterController>();
		if (m_CharController.IsPlayer)
		{
			enabled = false;
		}
		else
		{
			m_LOSLayers = LayerMask.GetMask("Default");
			SetProfile(m_DefaultProfile);
		}
	}

	void Update()
	{
		m_DecisionTimer -= Time.deltaTime;

		if (m_DecisionTimer < 0.0f)
		{
			m_DecisionTimer = m_Profile.m_TickRate;
			PumpBrain();
		}

#if UNITY_EDITOR
		switch (m_CurrentState)
		{
			case AIState.Idle:
				Debug.DrawLine(transform.position, transform.position + Vector3.up * 3.0f, Color.white);
				break;
			case AIState.FindingWeapon:
				Debug.DrawLine(transform.position, transform.position + Vector3.up * 3.0f, Color.yellow);
				break;
			case AIState.AttackingPlayer:
				Debug.DrawLine(transform.position, transform.position + Vector3.up * 3.0f, Color.green);
				break;
			case AIState.Fleeing:
				Debug.DrawLine(transform.position, transform.position + Vector3.up * 3.0f, Color.red);
				break;
		}
#endif

		switch (m_CurrentState)
		{
			case AIState.Idle:
				Update_Idle();
				break;
			case AIState.FindingWeapon:
				Update_FindingWeapon();
				break;
			case AIState.AttackingPlayer:
				Update_Attacking();
				break;
			case AIState.Fleeing:
				Update_Fleeing();
				break;
		}

		if (m_MoveToTarget)
			UpdateMovement();
		
		UpdateAiming();

		// Update LOS
		m_HasLineOfSight = false;
		if (!m_Profile.m_RequireLOS)
		{
			m_HasLineOfSight = true;
		}
		else if(m_AimToTarget)
		{
			Vector3 towardsTarget = (m_TargetAimPosition - transform.position);
			float maxScale = 0.9f;
			float minScale = 0.1f;

			float maxDistance = towardsTarget.magnitude * (maxScale - minScale);
			towardsTarget = towardsTarget.normalized;

			// Too close
			if (maxDistance < 1.0f)
			{
				m_HasLineOfSight = true;
			}
			else
			{
				// Can't see, so check can see
				Ray checkRay = new Ray(transform.position + towardsTarget * minScale, towardsTarget);
				RaycastHit hit;
				m_HasLineOfSight = !Physics.Raycast(checkRay, out hit, maxDistance);

#if UNITY_EDITOR
				if (m_HasLineOfSight)
				{
					Debug.DrawRay(checkRay.origin, checkRay.direction * maxDistance, Color.green);
				}
				else
				{
					Debug.DrawRay(checkRay.origin, checkRay.direction * hit.distance, Color.red);
				}
#endif
			}
		}
	}

	private void UpdateMovement()
	{
		if (m_TargetMoveTransform)
			m_TargetMovePosition = m_TargetMoveTransform.position;

		Vector3 diff = m_TargetMovePosition - transform.position;

		if (diff.sqrMagnitude <= 0.1f)
		{
			m_MoveToTarget = false;
			m_TargetMoveTransform = null;
		}
		else
		{
			Vector2 inputDir = new Vector2(diff.x, diff.z).normalized;
			m_CharController.Movement.Move(inputDir, m_MoveUrgency);
		}

#if UNITY_EDITOR
		if (m_MoveToTarget)
		{
			Debug.DrawLine(transform.position, m_TargetMovePosition, Color.blue);
		}
#endif
	}

	private void UpdateAiming()
	{
		if (m_AimToTarget)
		{
			if (m_TargetAimTransform)
				m_TargetAimPosition = m_TargetAimTransform.position;
		}
		else if (m_MoveToTarget)
			m_TargetAimPosition = m_TargetMovePosition;
		
		m_CharController.AimPosition = new Vector3(m_TargetAimPosition.x, transform.position.y, m_TargetAimPosition.z);
	}

	public void SetProfile(AIProfile profile)
	{
		if (!gameObject.activeInHierarchy && m_Profile == null)
		{
			// Defer it
			m_DefaultProfile = profile;
		}
		else
		{
			m_Profile = profile;
			m_DecisionTimer = 0.0f;

			CharacterMovement movement = GetComponent<CharacterMovement>();
			Debug.Assert(movement != null, "Unable to fetch CharacterMovement");
			movement.ApplySettings(profile);
		}
	}

	private void PumpBrain()
	{
		// Update useful states
		float weaponSearchDistSq = m_Profile.m_WeaponSearchMaxDistance * m_Profile.m_WeaponSearchMaxDistance;

		m_DiscoveredPlayer = GameObject.FindGameObjectWithTag("Player");
		if (m_DiscoveredPlayer != null && !m_DiscoveredPlayer.activeInHierarchy)
			m_DiscoveredPlayer = null;

		m_DiscoveredWeapons = GameObject.FindGameObjectsWithTag("Weapon").Where((w) =>
		{
			if (w != null && w.activeInHierarchy)
			{
				float distSq = GetTrackingDistanceSq(w);
				if (distSq <= weaponSearchDistSq)
				{
					WeaponController weapon = w.GetComponent<WeaponController>();
					return (weapon != null && !weapon.HasOwner);
				}
			}
			return false;
		}).OrderBy((w) => GetTrackingDistanceSq(w));

		// Decide state
		AIState previousState = m_CurrentState;
		m_BehaviourNoise = Random.value;

		m_CurrentState = AIState.Idle;

		if (m_CharController.WeaponCount < m_Profile.m_MinumumWeaponCount)
		{
			if (m_DiscoveredWeapons.Any())
			{
				m_CurrentState = AIState.FindingWeapon;
				m_TargetMoveTransform = null;
			}
			else
			{
				m_CurrentState = AIState.Fleeing;
				m_TargetAimTransform = m_DiscoveredPlayer != null ? m_DiscoveredPlayer.transform : null;
			}
		}
		else
		{
			// Check if should attack player
			if (m_DiscoveredPlayer != null)
			{
				// Keep attacking player, once spotted
				float distSq = GetTrackingDistanceSq(m_DiscoveredPlayer);
				float spotRangeSq = m_Profile.m_SpotRange * m_Profile.m_SpotRange;
				if (previousState == AIState.AttackingPlayer || distSq <= spotRangeSq)
				{
					m_CurrentState = AIState.AttackingPlayer;
					m_TargetAimTransform = m_DiscoveredPlayer.transform;
				}
			}
		}

		if (m_CurrentState == AIState.Idle && m_CharController.WeaponCount < m_Profile.m_DesiredWeaponCount && m_DiscoveredWeapons.Any())
		{
			m_CurrentState = AIState.FindingWeapon;
			m_TargetMoveTransform = null;
		}

		// Reset tracking when state changes
		if (m_CurrentState != previousState)
		{
			m_MoveToTarget = false;
			m_AimToTarget = false;
			m_TargetMoveTransform = null;
			m_TargetAimTransform = null;
			m_MoveUrgency = 1.0f;
		}
	}

	private void Update_Idle()
	{
		if (m_BehaviourNoise >= m_Profile.m_IdleMoveThreshold && !m_MoveToTarget)
		{
			float leftOverNoise = (m_BehaviourNoise - m_Profile.m_IdleMoveThreshold) / (1.0f - m_Profile.m_IdleMoveThreshold);
			float lhsNoise = Mathf.Clamp01((leftOverNoise - 0.5f) / 0.5f);
			float rhsNoise = Mathf.Clamp01((leftOverNoise - 0.5f) / -0.5f);

			float lhsSign = lhsNoise > 0.5f ? 1.0f : -1.0f;
			float rhsSign = rhsNoise > 0.5f ? 1.0f : -1.0f;

			float rand0 = Random.Range(m_Profile.m_IdleMinMoveDistance, m_Profile.m_IdleMaxMoveDistance) * lhsSign;
			float rand1 = Random.Range(m_Profile.m_IdleMinMoveDistance, m_Profile.m_IdleMaxMoveDistance) * rhsSign;

			m_TargetMovePosition = GetTraversableSpot(transform.position - transform.forward * rand0 + transform.right * rand1);
			m_MoveUrgency = m_Profile.m_IdleMoveUrgency;
			m_MoveToTarget = true;

			// Prevent multiple idle movements per pump
			m_BehaviourNoise = 0.0f;
		}
	}

	private void Update_FindingWeapon()
	{
		// Hunt for new weapon
		if (!HasValidMoveTransform() && m_DiscoveredWeapons.Count() != 0)
		{
			GameObject targetWeapon = m_DiscoveredWeapons.First();

			if (targetWeapon != null && targetWeapon.activeInHierarchy)
			{
				m_MoveToTarget = true;
				m_TargetMoveTransform = targetWeapon.transform;
			}
		}

		// Weapon not found, so update decission
		if (!HasValidMoveTransform() || m_TargetMoveTransform.parent == null)
			m_DecisionTimer = 0.0f;
	}

	private void Update_Attacking()
	{
		m_MoveToTarget = false;
		m_AimToTarget = true;
		
		if (HasValidAimTransform())
		{
			float attackRange = Mathf.Min(m_CharController.AttackRange, m_Profile.m_MaxWeaponAttackRangeInCalc);

			float minAttackRange = m_Profile.m_WeaponAttackRangeConsideration * attackRange + m_Profile.m_MinAttackRangeOffset;
			float maxAttackRange = m_Profile.m_WeaponAttackRangeConsideration * attackRange + m_Profile.m_MaxAttackRangeOffset;
			float minAttackRangeSq = minAttackRange * minAttackRange;
			float maxAttackRangeSq = maxAttackRange * maxAttackRange;

			float distSq = GetTrackingDistanceSq(m_TargetAimTransform);

			bool tryStrafe = false;
			float strafeNoise = m_BehaviourNoise;

			// Move backwards, outside of min attack range
			if (distSq < minAttackRangeSq)
			{
				Vector3 towardsTarget = (m_TargetAimPosition - transform.position).normalized;
				Vector3 targetSpot = m_TargetAimPosition + -towardsTarget * (minAttackRange + maxAttackRange) * 0.5f;

				m_MoveToTarget = true;
				m_TargetMovePosition = GetTraversableSpot(targetSpot);
			}
			// Move forwards
			else if (distSq > maxAttackRangeSq)
			{
				Vector3 towardsTarget = (m_TargetAimPosition - transform.position).normalized;
				Vector3 targetSpot = m_TargetAimPosition - towardsTarget * (minAttackRange + maxAttackRange) * 0.5f;

				m_MoveToTarget = true;
				m_TargetMovePosition = GetTraversableSpot(targetSpot);
			}
			// Strafe
			else if(!m_MoveToTarget && m_BehaviourNoise >= m_Profile.m_StrafeThreshold)
			{			
				// Use leftover noise
				strafeNoise = (m_BehaviourNoise - m_Profile.m_StrafeThreshold) / (1.0f - m_Profile.m_StrafeThreshold);
				tryStrafe = true;
			}

			// Shoot
			if (distSq < maxAttackRangeSq)
			{
				bool shouldShoot = true;
				
				// Can't see, so check can see
				if (!m_HasLineOfSight)
				{
					tryStrafe = true;
					shouldShoot = false;
				}

				if (shouldShoot)
					m_CharController.FireAnyWeapon(true, m_Profile.m_ShootCooldownScale);
			}

			// Strafe
			if(tryStrafe)
			{
				Vector3 towardsTarget = (m_TargetAimPosition - transform.position).normalized;
				Vector3 rightOfTarget = Vector3.Cross(towardsTarget, Vector3.up);

				float strafeDir = strafeNoise > 0.5f ? 1.0f : -1.0f;
				Vector3 targetSpot = transform.position + rightOfTarget * Random.Range(m_Profile.m_StrafeMinDistance, m_Profile.m_StrafeMaxDistance) * strafeDir;

				m_MoveToTarget = true;
				m_TargetMovePosition = GetTraversableSpot(targetSpot);
			}
		}
	}

	private void Update_Fleeing()
	{
		m_AimToTarget = true;

		// Flee from current target
		if (HasValidAimTransform() && !m_MoveToTarget)
		{
			float distSq = GetTrackingDistanceSq(m_TargetAimTransform);
			float maxDistSq = m_Profile.m_MaxFleeRange * m_Profile.m_MaxFleeRange;

			if (distSq <= maxDistSq)
			{
				// Select random spot behind target
				Vector3 towardsTarget = (m_TargetAimPosition - transform.position).normalized;
				Vector3 rightOfTarget = Vector3.Cross(towardsTarget, Vector3.up);

				float sign = m_BehaviourNoise > 0.5f ? 1.0f : -1.0f;

				float rand0 = Random.Range(m_Profile.m_MinFleeRange, m_Profile.m_MaxFleeRange);
				float rand1 = Random.Range(m_Profile.m_MinFleeRange, m_Profile.m_MaxFleeRange);

				m_TargetMovePosition = GetTraversableSpot(transform.position - towardsTarget * rand0 + rightOfTarget * rand1 * sign);
				m_MoveToTarget = true;
			}
		}
	}

	public void OnDamaged(GameObject source)
	{
		// Always update decision on damage
		if (enabled)
		{
			m_DecisionTimer = 0.0f;
		}
	}

	private bool HasValidMoveTransform()
	{
		return m_TargetMoveTransform != null && m_TargetMoveTransform.gameObject.activeInHierarchy;
	}

	private bool HasValidAimTransform()
	{
		return m_TargetAimTransform != null && m_TargetAimTransform.gameObject.activeInHierarchy;
	}

	private float GetTrackingDistanceSq(GameObject other)
	{
		return GetTrackingDistanceSq(other.transform.position);
	}

	private float GetTrackingDistanceSq(Transform other)
	{
		return GetTrackingDistanceSq(other.position);
	}

	private float GetTrackingDistanceSq(Vector3 positon)
	{
		Vector3 diff = transform.position - positon;
		float distSq = Vector3.Dot(diff, diff);
		return distSq;
	}

	private Vector3 GetTraversableSpot(Vector3 desiredSpot)
	{
		// TODO - Hookup checks
		return desiredSpot;
	}

	public bool HasDefaultProfile
	{
		get { return m_DefaultProfile != null; }
	}
}
