using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(SphereCollider))]
public class WeaponController : MonoBehaviour
{
	private Rigidbody m_Body;
	private CapsuleCollider m_Collider;
	private SphereCollider m_Trigger;
	private CharacterController m_Owner;

	private float m_CurrentCooldown;

	[SerializeField]
	private WeaponSettings m_Settings;
	[SerializeField]
	private GameObject m_ProjectileType;
	[SerializeField]
	private Transform[] m_ProjectileSockets;

	void Start()
    {
		m_Body = GetComponent<Rigidbody>();
		m_Collider = GetComponent<CapsuleCollider>();
		m_Trigger = GetComponent<SphereCollider>();
		SetPhysicsMode(true);

		if (m_ProjectileSockets.Length == 0)
			Debug.Log("No projectile sockets for " + gameObject.name);
	}

	void Update()
	{
		if (m_CurrentCooldown > 0.0f)
		{
			m_CurrentCooldown = Mathf.Max(0.0f, m_CurrentCooldown - Time.deltaTime);
		}
	}

	public bool CanFire
	{
		get { return m_CurrentCooldown == 0.0f; }
	}

	private void SetPhysicsMode(bool enabled)
	{
		m_Collider.enabled = enabled;
		m_Trigger.enabled = enabled;
		
		// The best way to do this is to literally delete the component and re-add when needed because transform doesn't get applied when physics system is at work :(
		if (!enabled)
		{
			Destroy(m_Body);
			m_Body = null;
		}
		else
		{
			m_Body = gameObject.AddComponent<Rigidbody>();
		}
	}

	public void OnPickup(CharacterController controller)
	{
		SetPhysicsMode(false);
		m_Owner = controller;
		m_Owner.OnCollectWeapon(this);
	}

	public void OnDrop(Vector3 sprayDirection)
	{
		VoxelModel model = GetComponentInChildren<VoxelModel>();
		if (model != null)
		{
			model.CreateDebris(sprayDirection);
		}

		Destroy(gameObject);
	}

	private void OnTriggerEnter(Collider other)
	{
		CharacterController controller = other.gameObject.GetComponent<CharacterController>();
		if (m_Owner == null && controller != null)
		{
			OnPickup(controller);
		}
	}

	public bool TryFire(bool buttonJustPressed)
	{
		if (m_Settings.m_Automatic || buttonJustPressed)
		{
			if (CanFire)
			{
				for (int i = 0; i < m_ProjectileSockets.Length; ++i)
				{
					Transform socket = m_ProjectileSockets[i];
					ProjectileController.LaunchProjectile(m_ProjectileType, m_Owner.gameObject, socket.position, new Vector3(m_Owner.AimDirection.x, 0, m_Owner.AimDirection.y).normalized);
				}

				m_CurrentCooldown = m_Settings.m_Cooldown;
				return true;
			}
		}

		return false;
	}
}
