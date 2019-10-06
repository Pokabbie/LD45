using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
	[SerializeField]
	private Text m_ScoreText;
	[SerializeField]
	private Text m_HealthText;


	void Start()
    {
        
    }
	
    void Update()
    {
		GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
		m_HealthText.text = "0";
		if (playerObj != null)
		{
			CharacterController character = playerObj.GetComponent<CharacterController>();
			if (character != null)
				m_HealthText.text = character.WeaponCount + " weapons";
		}

		m_ScoreText.text = GameController.Main.CurrentScore + " points";

	}
}
