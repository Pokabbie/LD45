using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class CharacterController : MonoBehaviour
{
	private CharacterMovement m_Movement;
	private Vector3 m_AimPosition = Vector2.right;
	private Vector2 m_AimDirection = Vector2.right;

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
}
