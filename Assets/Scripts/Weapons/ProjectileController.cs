using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ProjectileController : MonoBehaviour
{
	private Rigidbody m_Body;

	[SerializeField]
	private ProjectileSettings m_Settings;

	// Max checks
	private const int c_MaxChecks = 5;
	private const float c_StepSize = 0.5f;
	private const float c_CheckSize = 0.05f;

	private int m_HitCount = 0;
	private float m_LifeTime = 0;

	void Start()
    {
		m_Body = GetComponent<Rigidbody>();
	}
	
    void Update()
    {
		m_Body.velocity = transform.forward * m_Settings.m_Speed * Time.deltaTime;
		m_LifeTime += Time.deltaTime;

		if (m_LifeTime > m_Settings.m_LifeTime)
			DestroySelf();
	}

	public static GameObject LaunchProjectile(GameObject baseObj, GameObject owner, Vector3 origin, Vector3 direction)
	{
		Vector3 safeOrigin = origin + direction * c_StepSize;

		for (int i = 0; i < c_MaxChecks; ++i)
		{
			if (Physics.CheckSphere(safeOrigin, c_CheckSize))
			{
				safeOrigin += direction * c_StepSize;
			}
		}

		GameObject newProj = Instantiate(baseObj, safeOrigin, Quaternion.LookRotation(direction, Vector3.up));
		return newProj;
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (++m_HitCount > m_Settings.m_BounceCount)
		{
			DestroySelf();
		}
		else
		{
			transform.forward = Vector3.Reflect(transform.forward, collision.contacts[0].normal);
		}
	}

	public void DestroySelf()
	{
		Destroy(gameObject);
	}
}
