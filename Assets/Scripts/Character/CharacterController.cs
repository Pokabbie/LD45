using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class CharacterController : MonoBehaviour
{
	private CharacterMovement m_Movement;

	void Start()
    {
		m_Movement = GetComponent<CharacterMovement>();
	}
	
    void Update()
    {
        
    }

	public CharacterMovement Movement
	{
		get { return m_Movement; }
	}
}
