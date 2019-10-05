using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
	private Rigidbody m_Body;
	private Vector2 m_Velocity;
	private Vector2 m_CurrentInput;

	[SerializeField]
	private float m_MoveSpeed = 1.0f;
	
	[SerializeField]
	private float m_MaxSpeed = 10.0f;

	[SerializeField]
	private float m_Decay = 0.5f;


	void Start()
    {
		m_Body = GetComponent<Rigidbody>();
	}
	
    void Update()
    {
		Vector2 frameInput = m_CurrentInput * m_MoveSpeed;
		m_Velocity = Vector2.ClampMagnitude(m_Velocity + frameInput, m_MaxSpeed);

		Vector2 frameVelocity = m_Velocity * Time.deltaTime;
		m_Body.velocity = new Vector3(frameVelocity.x, m_Body.velocity.y, frameVelocity.y);

		m_Velocity = Vector2.Lerp(m_Velocity, Vector2.zero, m_Decay * Time.deltaTime);
		m_CurrentInput = Vector2.zero;
	}
	
	public void Move(Vector2 scaledDirection)
	{
		m_CurrentInput += scaledDirection;
	}
}
