using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	private CharacterController m_CharController;
	private Vector3 m_CursorPosition;

	public Transform testCursor;

	void Start()
    {
		if (gameObject.CompareTag("Player"))
		{
			m_CharController = GetComponent<CharacterController>();
		}
		else
		{
			enabled = false;
		}
	}
	
    void Update()
	{
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

		UpdateAimPosition();

		// DEBUG 
		//m_CursorPosition = transform.position + (Quaternion.AngleAxis(Time.time * 15.0f, Vector3.up) * Vector3.right) * 3.0f;

		m_CharController.AimPosition = m_CursorPosition;

		// DEBUG
		testCursor.transform.position = m_CursorPosition;
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
