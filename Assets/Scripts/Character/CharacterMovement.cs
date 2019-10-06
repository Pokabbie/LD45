using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
	private Rigidbody m_Body;
	private Vector2 m_Velocity;
	private Vector2 m_CurrentInput;
	private float m_CurrentUrgency;

	private Quaternion m_TargetRotation;
	private bool m_IsFacingRight = true;

	private float m_MaxSpeedFactor = 1.0f;
	private float m_MoveSpeedFactor = 1.0f;

	[Header("Movement")]
	[SerializeField]
	private float m_MoveSpeed = 1.0f;
	
	[SerializeField]
	private float m_MaxSpeed = 10.0f;

	[SerializeField]
	private float m_Decay = 0.5f;

	[Header("Animation")]
	[SerializeField]
	private bool m_ShouldAnimate = true;

	[SerializeField]
	private float m_WoobleFrequency = 15.0f;

	[SerializeField]
	private float m_WoobleAngle = 3.0f;

	[SerializeField]
	private float m_WoobleJump = 0.5f;

	[SerializeField]
	private float m_WoobleMoveScale = 0.5f;

	[SerializeField]
	private float m_FlipSpeed = 0.5f;

	[SerializeField]
	private Transform m_PositionAnimationTarget;
	[SerializeField]
	private Transform m_RotationAnimationTarget;

	private Vector2 m_AnimationVelocity;
	private Vector3 m_BaseLocation;
	private Quaternion m_BaseRotation;


	void Start()
    {
		m_Body = GetComponent<Rigidbody>();
		m_BaseLocation = m_PositionAnimationTarget.localPosition;
		m_BaseRotation = m_RotationAnimationTarget.rotation;
		m_TargetRotation = Quaternion.AngleAxis(0.0f, Vector3.up) * m_BaseRotation;
	}
	
    void Update()
    {
		Vector3 originalVelocity = m_Velocity;

		Vector2 frameInput = m_CurrentInput * m_MoveSpeed * m_MoveSpeedFactor;
		m_Velocity = Vector2.ClampMagnitude(m_Velocity + frameInput, m_MaxSpeed * m_MaxSpeedFactor * Mathf.Clamp01(m_CurrentUrgency));

		Vector2 frameVelocity = m_Velocity * Time.deltaTime;
		m_Body.velocity = new Vector3(frameVelocity.x, m_Body.velocity.y, frameVelocity.y);

		m_Velocity = Vector2.Lerp(m_Velocity, Vector2.zero, m_Decay * Time.deltaTime);
		m_CurrentInput = Vector2.zero;
		m_CurrentUrgency = 0.0f;

		if (m_ShouldAnimate)
			UpdateAnimation();

		
	}

	private void UpdateAnimation()
	{
		Vector3 previousVelocity = m_AnimationVelocity;
		m_AnimationVelocity = Vector3.Lerp(m_AnimationVelocity, m_Velocity, 50.0f * Time.deltaTime);

		// Look rotation animation
		if (Mathf.Abs(m_AnimationVelocity.x) > 0.01f && Mathf.Sign(m_AnimationVelocity.x) != Mathf.Sign(previousVelocity.x))
		{
			if (m_AnimationVelocity.x > 0.0f)
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
		if (Mathf.Abs(m_RotationAnimationTarget.localRotation.eulerAngles.y - m_TargetRotation.eulerAngles.y) > 5.0f)
		{
			m_RotationAnimationTarget.localRotation = Quaternion.Lerp(m_RotationAnimationTarget.localRotation, m_TargetRotation, m_FlipSpeed * Time.deltaTime);
		}

		// Apply wobble
		float wobbleValue = Mathf.Sin(Time.time * m_WoobleFrequency) * Mathf.Clamp01(m_AnimationVelocity.magnitude * m_WoobleMoveScale);
		m_RotationAnimationTarget.localRotation = Quaternion.AngleAxis(wobbleValue * m_WoobleAngle, Vector3.forward) * m_RotationAnimationTarget.localRotation;

		m_PositionAnimationTarget.localPosition = m_BaseLocation + new Vector3(0, Mathf.Abs(wobbleValue) * m_WoobleJump, 0);
	}

	public void ApplySettings(AIProfile profile)
	{
		m_MaxSpeedFactor = profile.m_MaxSpeedFactor;
		m_MoveSpeedFactor = profile.m_MoveSpeedFactor;
	}
	
	public void Move(Vector2 scaledDirection, float urgency = 1.0f)
	{
		m_CurrentUrgency += urgency;
		m_CurrentInput += scaledDirection;
	}

	public bool IsFacingRight
	{
		get { return m_IsFacingRight; }
	}
}
