using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	private CharacterController m_CharController;

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
	}
}
