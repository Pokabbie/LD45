using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
	private Rigidbody m_Body;
	private Vector2 m_Velocity;
	private Vector2 m_CurrentInput;

	private Quaternion m_TargetRotation;
	private bool m_IsFacingRight = true;
	
	[Header("Movement")]
	[SerializeField]
	private float m_MoveSpeed = 1.0f;
	
	[SerializeField]
	private float m_MaxSpeed = 10.0f;

	[SerializeField]
	private float m_Decay = 0.5f;

	[Header("Animation")]
	[SerializeField]
	private float m_FlipSpeed = 0.5f;

	[SerializeField]
	private Transform m_AnimationTarget;
	private Quaternion m_BaseRotation;


	void Start()
    {
		m_Body = GetComponent<Rigidbody>();

		m_BaseRotation = m_AnimationTarget.rotation;
		m_TargetRotation = Quaternion.AngleAxis(0.0f, Vector3.up) * m_BaseRotation;
	}
	
    void Update()
    {
		Vector3 originalVelocity = m_Velocity;

		Vector2 frameInput = m_CurrentInput * m_MoveSpeed;
		m_Velocity = Vector2.ClampMagnitude(m_Velocity + frameInput, m_MaxSpeed);

		Vector2 frameVelocity = m_Velocity * Time.deltaTime;
		m_Body.velocity = new Vector3(frameVelocity.x, m_Body.velocity.y, frameVelocity.y);

		m_Velocity = Vector2.Lerp(m_Velocity, Vector2.zero, m_Decay * Time.deltaTime);
		m_CurrentInput = Vector2.zero;

		// Rotation animation
		if (Mathf.Abs(m_Velocity.x) > 0.01f && Mathf.Sign(m_Velocity.x) != Mathf.Sign(originalVelocity.x))
		{
			if (m_Velocity.x > 0.0f)
			{
				m_TargetRotation = Quaternion.AngleAxis(0.0f, Vector3.up) * m_BaseRotation;
				m_IsFacingRight = true;
			}
			else
			{
				m_TargetRotation = Quaternion.AngleAxis(180.0f, Vector3.up) * m_BaseRotation;
				m_IsFacingRight = false; 
			}
		}

		// Make it stable
		if (Mathf.Abs(m_AnimationTarget.localRotation.eulerAngles.y - m_TargetRotation.eulerAngles.y) > 5.0f)
		{
			m_AnimationTarget.localRotation = Quaternion.Slerp(m_AnimationTarget.localRotation, m_TargetRotation, m_FlipSpeed * Time.deltaTime);
		}
	}
	
	public void Move(Vector2 scaledDirection)
	{
		m_CurrentInput += scaledDirection;
	}

	public bool IsFacingRight
	{
		get { return m_IsFacingRight; }
	}
}
