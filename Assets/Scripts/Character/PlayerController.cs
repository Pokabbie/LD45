using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	private CharacterController m_CharController;
	private Vector3 m_CursorPosition;
	
	void Start()
	{
		m_CharController = GetComponent<CharacterController>();
		if (!m_CharController.IsPlayer)
		{
			enabled = false;
		}
	}
	
    void Update()
	{
		// DEBUG 
		//m_CursorPosition = transform.position + (Quaternion.AngleAxis(Time.time * 15.0f, Vector3.up) * Vector3.right) * 3.0f;

		UpdateAimPosition();
		m_CharController.AimPosition = m_CursorPosition;

		// DEBUG
		//testCursor.transform.position = m_CursorPosition;

		// Movement
		if (Input.GetKey(KeyCode.A))
		{
			m_CharController.Movement.Move(Vector2.left);
		}
		if (Input.GetKey(KeyCode.D))
		{
			m_CharController.Movement.Move(Vector2.right);
		}
		if (Input.GetKey(KeyCode.W))
		{
			m_CharController.Movement.Move(Vector2.up);
		}
		if (Input.GetKey(KeyCode.S))
		{
			m_CharController.Movement.Move(Vector2.down);
		}

		// Shooting
		if (Input.GetMouseButton(0))
		{
			m_CharController.FireAnyWeapon(Input.GetMouseButtonDown(0));
		}
	}

	void UpdateAimPosition()
	{
		// Intersect mouse with player plain
		Vector3 camPosition = Camera.main.transform.position;
		Vector3 mouseDirection = Camera.main.ScreenPointToRay(Input.mousePosition).direction;
		
		Vector3 planeOrigin = transform.position;
		Vector3 planeNormal = Vector3.up;

		float d = Vector3.Dot((planeOrigin - camPosition), planeNormal) / Vector3.Dot(mouseDirection, planeNormal);
		m_CursorPosition = camPosition + mouseDirection * d;
	}
}
