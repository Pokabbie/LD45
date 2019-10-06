using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverScreen : MonoBehaviour
{
	private float m_WaitCooldown;

	void Awake()
	{
		m_WaitCooldown = 0.5f;
	}

	void Start()
    {
		gameObject.SetActive(false);   
    }
	
    void Update()
    {
		m_WaitCooldown -= Time.deltaTime;

		if (m_WaitCooldown <= 0.0f)
		{
			if (Input.anyKeyDown)
			{
				GameController.Main.Restart();
				gameObject.SetActive(false);
			}
		}
    }
}
