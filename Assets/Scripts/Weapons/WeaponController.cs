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

	void Start()
    {
		m_Body = GetComponent<Rigidbody>();
		m_Collider = GetComponent<CapsuleCollider>();
		m_Trigger = GetComponent<SphereCollider>();
		SetPhysicsMode(true);
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

	public void OnDrop()
	{
		m_Owner = null;
		SetPhysicsMode(true);
	}

	private void OnTriggerEnter(Collider other)
	{
		CharacterController controller = other.gameObject.GetComponent<CharacterController>();
		if (m_Owner == null && controller != null)
		{
			OnPickup(controller);
		}
	}
}
