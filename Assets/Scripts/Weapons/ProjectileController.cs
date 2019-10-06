using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ProjectileController : MonoBehaviour
{
	private Rigidbody m_Body;
	private GameObject m_Owner;

	[SerializeField]
	private ProjectileSettings m_Settings;

	// Max checks
	private const int c_MaxChecks = 0;
	private const float c_StepSize = 0.5f;
	private const float c_CheckSize = 0.05f;

	private const float c_SelfDamageTimeTheshold = 0.1f;

	private int m_HitCount;
	private float m_LifeTime;

	void Awake()
    {
		m_Body = GetComponent<Rigidbody>();
		m_Owner = null;
		m_HitCount = 0;
		m_LifeTime = 0.0f;
	}
	
    void Update()
    {
		m_Body.velocity = transform.forward * m_Settings.m_Speed * Time.deltaTime;
		m_LifeTime += Time.deltaTime;

		if (m_LifeTime > m_Settings.m_LifeTime)
			DestroySelf();
	}

	public static ProjectileController LaunchProjectile(GameObject baseObj, GameObject owner, Vector3 origin, Vector3 direction)
	{
		Vector3 safeOrigin = origin;

		for (int i = 0; i < c_MaxChecks; ++i)
		{
			if (Physics.CheckSphere(safeOrigin, c_CheckSize))
			{
				safeOrigin += direction * c_StepSize;
			}
		}

		GameObject newProj = Instantiate(baseObj, safeOrigin, Quaternion.LookRotation(direction, Vector3.up));
		ProjectileController proj = newProj.GetComponentInChildren<ProjectileController>();
		proj.m_Owner = owner;
		return proj;
	}

	public void OnCollisionEnter(Collision collision)
	{
		// Prevent self damage on initial shot
		if (m_LifeTime > c_SelfDamageTimeTheshold || m_Owner != collision.gameObject)// || !m_Owner.CompareTag("Player"))
		{
			Damageable damageable = collision.gameObject.GetComponent<Damageable>();

			if (damageable != null)
			{
				damageable.ApplyDamage(gameObject);
			}

			if (++m_HitCount > m_Settings.m_BounceCount)
			{
				DestroySelf();
			}
			else
			{
				transform.forward = Vector3.Reflect(transform.forward, collision.contacts[0].normal);
			}
		}
	}

	public void DestroySelf()
	{
		Destroy(gameObject);
	}

	public float Range
	{
		get
		{
			return m_Settings.m_LifeTime * m_Settings.m_Speed;
		}
	}

	public bool DoesBounce
	{
		get
		{
			return m_Settings.m_BounceCount != 0;
		}
	}
}
