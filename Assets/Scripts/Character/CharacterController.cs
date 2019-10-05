using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(Damageable))]
public class CharacterController : MonoBehaviour
{
	private CharacterMovement m_Movement;
	private Vector3 m_AimPosition = Vector2.right;
	private Vector2 m_AimDirection = Vector2.right;
	
	private List<WeaponController> m_Weapons = new List<WeaponController>();
	private WeaponController m_LeftHandWeapon;
	private WeaponController m_RightHandWeapon;

	[Header("Equipment")]
	[SerializeField]
	private Transform m_BackpackSocket;
	[SerializeField]
	private float m_BackpackJitter = 0.0f;

	[Header("Left Hand")]
	[SerializeField]
	private Transform m_LeftHandTransform;
	[SerializeField]
	private Transform m_LeftHandMaxTransform;
	[SerializeField]
	private Transform m_LeftHandMinTransform;
	[SerializeField]
	private Transform m_LeftHandSocket;

	[Header("Right Hand")]
	[SerializeField]
	private Transform m_RightHandTransform;
	[SerializeField]
	private Transform m_RightHandMaxTransform;
	[SerializeField]
	private Transform m_RightHandMinTransform;
	[SerializeField]
	private Transform m_RightHandSocket;

	void Start()
    {
		m_Movement = GetComponent<CharacterMovement>();
	}
	
    void Update()
    {
		// Update left hand animation
		{
			Vector3 targetPositon = Vector3.Slerp(m_LeftHandMinTransform.localPosition, m_LeftHandMaxTransform.localPosition, (m_AimDirection.y + 1.0f) * 0.5f);
			m_LeftHandTransform.localPosition = targetPositon;
			m_LeftHandTransform.LookAt(m_AimPosition);
		}

		// Update right hand animation
		{
			Vector3 targetPositon = Vector3.Slerp(m_RightHandMinTransform.localPosition, m_RightHandMaxTransform.localPosition, (m_AimDirection.y + 1.0f) * 0.5f);
			m_RightHandTransform.localPosition = targetPositon;
			m_RightHandTransform.LookAt(m_AimPosition);
		}
	}
	
	public CharacterMovement Movement
	{
		get { return m_Movement; }
	}

	public bool IsPlayer
	{
		get { return gameObject.CompareTag("Player"); }
	}

	public Vector2 AimDirection
	{
		get { return m_AimDirection; }
		set
		{
			m_AimDirection = value.normalized;
			m_AimPosition = transform.position + new Vector3(m_AimDirection.x, 0, m_AimDirection.y) * 3.0f;
		}
	}

	public Vector3 AimPosition
	{
		get { return m_AimPosition; }
		set
		{
			m_AimPosition = value;
			m_AimDirection = new Vector2(m_AimPosition.x - transform.position.x, m_AimPosition.z - transform.position.z).normalized;
		}
	}

	private IEnumerable<WeaponController> UnequipedWeapons
	{
		get { return m_Weapons.Where((w) => w != m_RightHandWeapon && w != m_LeftHandWeapon); }
	}

	public void OnCollectWeapon(WeaponController weapon)
	{
		AddWeapon(weapon);
	}

	public void AddWeapon(WeaponController weapon)
	{
		m_Weapons.Add(weapon);

		if (!TryAddToLeftHand(weapon) && !TryAddToRightHand(weapon))
		{
			PlaceOnBackpack(weapon);
		}
	}

	private bool TryAddToLeftHand(WeaponController weapon)
	{
		if (m_LeftHandWeapon == null)
		{
			weapon.transform.SetParent(m_LeftHandSocket);
			weapon.transform.localPosition = Vector3.zero;
			weapon.transform.localRotation = Quaternion.identity;
			m_LeftHandWeapon = weapon;
			return true;
		}

		return false;
	}

	private bool TryAddToRightHand(WeaponController weapon)
	{
		if (m_RightHandWeapon == null)
		{
			weapon.transform.SetParent(m_RightHandSocket);
			weapon.transform.localPosition = Vector3.zero;
			weapon.transform.localRotation = Quaternion.identity;
			m_RightHandWeapon = weapon;
			return true;
		}

		return false;
	}

	private void PlaceOnBackpack(WeaponController weapon)
	{
		weapon.transform.SetParent(m_BackpackSocket);
		weapon.transform.localPosition = Vector3.zero + new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f) * 2.0f * m_BackpackJitter;
		weapon.transform.localRotation = Quaternion.identity * Quaternion.AngleAxis(Random.value * 360.0f, Vector3.forward);
	}

	public bool FireAnyWeapon(bool buttonJustPressed)
	{
		bool fired = false;
		if (m_LeftHandWeapon != null)
			fired |= m_LeftHandWeapon.TryFire(buttonJustPressed);

		if (m_RightHandWeapon != null)
			fired |= m_RightHandWeapon.TryFire(buttonJustPressed);

		return fired;
	}

	public void DropWeapon(WeaponController weapon, Vector3 sprayDirection)
	{
		m_Weapons.Remove(weapon);

		if (m_LeftHandWeapon == weapon)
			m_LeftHandWeapon = null;
		if (m_RightHandWeapon == weapon)
			m_RightHandWeapon = null;
		
		// Try refill slot
		var leftOvers = UnequipedWeapons;
		if (leftOvers.Any())
		{
			if (!TryAddToLeftHand(leftOvers.First()))
				TryAddToRightHand(leftOvers.First());
		}

		weapon.OnDrop(sprayDirection);
	}

	public void OnDamaged(GameObject source)
	{
		Vector3 sprayDirection = source.transform.forward;

		// No weapons remaining
		if (m_LeftHandWeapon == null && m_RightHandWeapon == null)
		{
			VoxelModel model = GetComponentInChildren<VoxelModel>();
			if (model != null)
			{
				Vector3 direction = transform.position - source.transform.position;
				model.CreateDebris(direction);
			}

			// DED
			Destroy(gameObject); // Broadcast?
		}
		// Only 1 weapon in right hand
		else if (m_LeftHandWeapon == null)
		{
			DropWeapon(m_RightHandWeapon, sprayDirection);
		}
		// Only 1 weapon in left hand
		else if (m_RightHandWeapon == null)
		{
			DropWeapon(m_LeftHandWeapon, sprayDirection);
		}
		// Both weapons, so pick at random
		else
		{
			int rand = Random.Range(0, 2);

			if (rand == 0)
				DropWeapon(m_RightHandWeapon, sprayDirection);
			else
				DropWeapon(m_LeftHandWeapon, sprayDirection);
		}
	}
}
