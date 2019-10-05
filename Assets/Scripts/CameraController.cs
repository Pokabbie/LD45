using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField]
	private Transform m_FocusPoint;

	[SerializeField]
	private float m_Snappiness = 1.0f;

	private Transform m_CurrentTarget;

    void Start()
    {
        
    }
	
    void Update()
    {
		if (m_CurrentTarget == null)
		{
			GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
			if (playerObj != null)
			{
				m_CurrentTarget = playerObj.transform;
			}
		}

		if (m_CurrentTarget != null)
		{
			Vector3 offset = transform.position - m_FocusPoint.position;

			transform.position = Vector3.Slerp(transform.position, m_CurrentTarget.position + offset, m_Snappiness * Time.deltaTime);
		}
    }
}
